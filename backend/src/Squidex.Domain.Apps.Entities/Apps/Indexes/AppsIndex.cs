// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Squidex.Caching;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsIndex : IAppsIndex, ICommandMiddleware
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
        private readonly IAppRepository appRepository;
        private readonly IGrainFactory grainFactory;
        private readonly IReplicatedCache grainCache;

        public AppsIndex(IAppRepository appRepository, IGrainFactory grainFactory, IReplicatedCache grainCache)
        {
            this.appRepository = appRepository;
            this.grainFactory = grainFactory;
            this.grainCache = grainCache;
        }

        public Task RemoveReservationAsync(string? token,
            CancellationToken ct = default)
        {
            return Cache().RemoveReservationAsync(token);
        }

        public Task<string?> ReserveAsync(DomainId id, string name,
            CancellationToken ct = default)
        {
            return Cache().ReserveAsync(id, name);
        }

        public async Task<List<IAppEntity>> GetAppsForUserAsync(string userId, PermissionSet permissions,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("AppProvider/GetAppsForUserAsync"))
            {
                var ids =
                    await Task.WhenAll(
                        GetAppIdsByUserAsync(userId),
                        GetAppIdsAsync(permissions.ToAppNames()));

                var apps =
                    await Task.WhenAll(ids
                        .SelectMany(x => x).Distinct()
                        .Select(id => GetAppAsync(id, false, ct)));

                return apps.NotNull().ToList();
            }
        }

        public async Task<IAppEntity?> GetAppAsync(string name, bool canCache = false,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("AppProvider/GetAppByNameAsync"))
            {
                if (canCache)
                {
                    if (grainCache.TryGetValue(GetCacheKey(name), out var v) && v is IAppEntity cacheApp)
                    {
                        return cacheApp;
                    }
                }

                var appId = await GetAppIdAsync(name);

                if (appId == DomainId.Empty)
                {
                    return null;
                }

                return await GetAppAsync(appId, canCache, ct);
            }
        }

        public async Task<IAppEntity?> GetAppAsync(DomainId appId, bool canCache = false,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("AppProvider/GetAppAsync"))
            {
                if (canCache)
                {
                    if (grainCache.TryGetValue(GetCacheKey(appId), out var cached) && cached is IAppEntity cachedApp)
                    {
                        return cachedApp;
                    }
                }

                var app = await GetAppCoreAsync(appId);

                if (app != null)
                {
                    await CacheItAsync(app);
                }

                return app;
            }
        }

        private async Task<IReadOnlyCollection<DomainId>> GetAppIdsByUserAsync(string userId)
        {
            using (Telemetry.Activities.StartActivity("AppProvider/GetAppIdsByUserAsync"))
            {
                var result = await appRepository.QueryIdsAsync(userId);

                return result.Values;
            }
        }

        private async Task<IReadOnlyCollection<DomainId>> GetAppIdsAsync(string[] names)
        {
            using (Telemetry.Activities.StartActivity("AppProvider/GetAppIdsAsync"))
            {
                var result = await Cache().GetAppIdsAsync(names);

                return result;
            }
        }

        private async Task<DomainId> GetAppIdAsync(string name)
        {
            using (Telemetry.Activities.StartActivity("AppProvider/GetAppIdAsync"))
            {
                var result = await Cache().GetAppIdsAsync(new[] { name });

                return result.FirstOrDefault();
            }
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            var command = context.Command;

            if (command is CreateApp createApp)
            {
                var cache = Cache();

                var token = await CheckAppAsync(cache, createApp);
                try
                {
                    await next(context);
                }
                finally
                {
                    await cache.RemoveReservationAsync(token);
                }
            }
            else
            {
                await next(context);
            }

            if (context.IsCompleted)
            {
                switch (command)
                {
                    case CreateApp create:
                        await OnCreateAsync(create);
                        break;
                    case DeleteApp delete:
                        await OnDeleteAsync(delete);
                        break;
                    case AppUpdateCommand update:
                        await OnUpdateAsync(update);
                        break;
                }
            }
        }

        private async Task<string?> CheckAppAsync(IAppsCacheGrain cache, CreateApp command)
        {
            var token = await cache.ReserveAsync(command.AppId, command.Name);

            if (token == null)
            {
                throw new ValidationException(T.Get("apps.nameAlreadyExists"));
            }

            try
            {
                var existingId = await GetAppIdAsync(command.Name);

                if (existingId != default)
                {
                    throw new ValidationException(T.Get("apps.nameAlreadyExists"));
                }
            }
            catch
            {
                // Catch our own exception, juist in case something went wrong before.
                await cache.RemoveReservationAsync(token);
                throw;
            }

            return token;
        }

        private async Task OnCreateAsync(CreateApp create)
        {
            await InvalidateItAsync(create.AppId, create.Name);

            await Cache().AddAsync(create.AppId, create.Name);
        }

        private async Task OnDeleteAsync(DeleteApp delete)
        {
            await InvalidateItAsync(delete.AppId.Id, delete.AppId.Name);

            await Cache().RemoveAsync(delete.AppId.Id);
        }

        private async Task OnUpdateAsync(AppUpdateCommand update)
        {
            await InvalidateItAsync(update.AppId.Id, update.AppId.Name);
        }

        private IAppsCacheGrain Cache()
        {
            return grainFactory.GetGrain<IAppsCacheGrain>(SingleGrain.Id);
        }

        private async Task<IAppEntity?> GetAppCoreAsync(DomainId id, bool allowArchived = false)
        {
            var app = (await grainFactory.GetGrain<IAppGrain>(id.ToString()).GetStateAsync()).Value;

            if (app.Version <= EtagVersion.Empty || (app.IsDeleted && !allowArchived))
            {
                return null;
            }

            return app;
        }

        private static string GetCacheKey(DomainId id)
        {
            return $"{typeof(AppsIndex)}_Apps_Id_{id}";
        }

        private static string GetCacheKey(string name)
        {
            return $"{typeof(AppsIndex)}_Apps_Name_{name}";
        }

        private Task InvalidateItAsync(DomainId id, string name)
        {
            return grainCache.RemoveAsync(
                GetCacheKey(id),
                GetCacheKey(name));
        }

        private Task CacheItAsync(IAppEntity app)
        {
            return Task.WhenAll(
                grainCache.AddAsync(GetCacheKey(app.Id), app, CacheDuration),
                grainCache.AddAsync(GetCacheKey(app.Name), app, CacheDuration));
        }
    }
}
