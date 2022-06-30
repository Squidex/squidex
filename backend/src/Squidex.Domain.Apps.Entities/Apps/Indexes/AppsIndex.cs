// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Caching;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsIndex : IAppsIndex, ICommandMiddleware, IInitializable
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
        private readonly IAppRepository appRepository;
        private readonly IReplicatedCache appCache;
        private readonly IUniqueNamesState uniqueNamesState;

        public AppsIndex(IAppRepository appRepository, IReplicatedCache appCache, IUniqueNamesState uniqueNamesState)
        {
            this.appRepository = appRepository;
            this.appCache = appCache;
            this.uniqueNamesState = uniqueNamesState;
        }

        public Task InitializeAsync(
            CancellationToken ct)
        {
            return uniqueNamesState.LoadAsync(ct);
        }

        public Task RemoveReservationAsync(string? token,
            CancellationToken ct = default)
        {
            return uniqueNamesState.RemoveReservationAsync(token, ct);
        }

        public async Task<string?> ReserveAsync(DomainId id, string name,
            CancellationToken ct = default)
        {
            if (await appRepository.FindAsync(name, ct) != null)
            {
                return null;
            }

            return await uniqueNamesState.ReserveAsync(id, name, ct);
        }

        public async Task<List<IAppEntity>> GetAppsForUserAsync(string userId, PermissionSet permissions,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("AppsIndex/GetAppsForUserAsync"))
            {
                var appQueries =
                    await Task.WhenAll(
                        appRepository.QueryAllAsync(userId, ct),
                        appRepository.QueryAllAsync(permissions.ToAppNames(), ct));

                var apps = appQueries.SelectMany(x => x).NotNull().ToList();

                foreach (var app in apps)
                {
                    await CacheItAsync(app);
                }

                return apps;
            }
        }

        public async Task<IAppEntity?> GetAppAsync(string name, bool canCache = false,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("AppsIndex/GetAppByNameAsync"))
            {
                if (canCache)
                {
                    if (appCache.TryGetValue(GetCacheKey(name), out var v) && v is IAppEntity cacheApp)
                    {
                        return cacheApp;
                    }
                }

                var app = await appRepository.FindAsync(name, ct);

                if (app != null)
                {
                    await CacheItAsync(app);
                }

                return app;
            }
        }

        public async Task<IAppEntity?> GetAppAsync(DomainId appId, bool canCache = false,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("AppsIndex/GetAppAsync"))
            {
                if (canCache)
                {
                    if (appCache.TryGetValue(GetCacheKey(appId), out var cached) && cached is IAppEntity cachedApp)
                    {
                        return cachedApp;
                    }
                }

                var app = await appRepository.FindAsync(appId, ct);

                if (app != null)
                {
                    await CacheItAsync(app);
                }

                return app;
            }
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            var command = context.Command;

            if (command is CreateApp createApp)
            {
                var token = await CheckAppAsync(createApp);
                try
                {
                    await next(context);
                }
                finally
                {
                    await uniqueNamesState.RemoveReservationAsync(token);
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

        private async Task<string?> CheckAppAsync(CreateApp command)
        {
            var token = await ReserveAsync(command.AppId, command.Name);

            if (token == null)
            {
                throw new ValidationException(T.Get("apps.nameAlreadyExists"));
            }

            return token;
        }

        private async Task OnCreateAsync(CreateApp create)
        {
            await InvalidateItAsync(create.AppId, create.Name);
        }

        private async Task OnDeleteAsync(DeleteApp delete)
        {
            await InvalidateItAsync(delete.AppId.Id, delete.AppId.Name);
        }

        private async Task OnUpdateAsync(AppUpdateCommand update)
        {
            await InvalidateItAsync(update.AppId.Id, update.AppId.Name);
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
            return appCache.RemoveAsync(
                GetCacheKey(id),
                GetCacheKey(name));
        }

        private Task CacheItAsync(IAppEntity app)
        {
            return Task.WhenAll(
                appCache.AddAsync(GetCacheKey(app.Id), app, CacheDuration),
                appCache.AddAsync(GetCacheKey(app.Name), app, CacheDuration));
        }
    }
}
