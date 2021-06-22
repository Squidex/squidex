// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Squidex.Caching;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Log;
using Squidex.Shared;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsIndex : IAppsIndex, ICommandMiddleware
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
        private readonly IGrainFactory grainFactory;
        private readonly IReplicatedCache grainCache;

        public AppsIndex(IGrainFactory grainFactory, IReplicatedCache grainCache)
        {
            this.grainFactory = grainFactory;
            this.grainCache = grainCache;
        }

        public async Task RebuildByContributorsAsync(DomainId appId, HashSet<string> contributors)
        {
            foreach (var contributorId in contributors)
            {
                await Index(contributorId).AddAsync(appId);
            }
        }

        public Task RebuildByContributorsAsync(string contributorId, HashSet<DomainId> apps)
        {
            return Index(contributorId).RebuildAsync(apps);
        }

        public Task RebuildAsync(Dictionary<string, DomainId> appsByName)
        {
            return Index().RebuildAsync(appsByName);
        }

        public Task RemoveReservationAsync(string? token)
        {
            return Index().RemoveReservationAsync(token);
        }

        public Task<List<DomainId>> GetIdsAsync()
        {
            return Index().GetIdsAsync();
        }

        public Task<bool> AddAsync(string? token)
        {
            return Index().AddAsync(token);
        }

        public Task<string?> ReserveAsync(DomainId id, string name)
        {
            return Index().ReserveAsync(id, name);
        }

        public async Task<List<IAppEntity>> GetAppsAsync()
        {
            using (Profiler.TraceMethod<AppsIndex>())
            {
                var ids = await GetAppIdsAsync();

                var apps =
                    await Task.WhenAll(ids
                        .Select(id => GetAppAsync(id, false)));

                return apps.NotNull().ToList();
            }
        }

        public async Task<List<IAppEntity>> GetAppsForUserAsync(string userId, PermissionSet permissions)
        {
            using (Profiler.TraceMethod<AppsIndex>())
            {
                var ids =
                    await Task.WhenAll(
                        GetAppIdsByUserAsync(userId),
                        GetAppIdsAsync(permissions.ToAppNames()));

                var apps =
                    await Task.WhenAll(ids
                        .SelectMany(x => x).Distinct()
                        .Select(id => GetAppAsync(id, false)));

                return apps.NotNull().ToList();
            }
        }

        public async Task<IAppEntity?> GetAppByNameAsync(string name, bool canCache = false)
        {
            using (Profiler.TraceMethod<AppsIndex>())
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

                return await GetAppAsync(appId, canCache);
            }
        }

        public async Task<IAppEntity?> GetAppAsync(DomainId appId, bool canCache)
        {
            using (Profiler.TraceMethod<AppsIndex>())
            {
                if (canCache)
                {
                    if (grainCache.TryGetValue(GetCacheKey(appId), out var v) && v is IAppEntity cachedApp)
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

        private async Task<List<DomainId>> GetAppIdsByUserAsync(string userId)
        {
            using (Profiler.TraceMethod<AppProvider>())
            {
                return await grainFactory.GetGrain<IAppsByUserIndexGrain>(userId).GetIdsAsync();
            }
        }

        private async Task<List<DomainId>> GetAppIdsAsync()
        {
            using (Profiler.TraceMethod<AppProvider>())
            {
                return await grainFactory.GetGrain<IAppsByNameIndexGrain>(SingleGrain.Id).GetIdsAsync();
            }
        }

        private async Task<List<DomainId>> GetAppIdsAsync(string[] names)
        {
            using (Profiler.TraceMethod<AppProvider>())
            {
                return await grainFactory.GetGrain<IAppsByNameIndexGrain>(SingleGrain.Id).GetIdsAsync(names);
            }
        }

        private async Task<DomainId> GetAppIdAsync(string name)
        {
            using (Profiler.TraceMethod<AppProvider>())
            {
                return await grainFactory.GetGrain<IAppsByNameIndexGrain>(SingleGrain.Id).GetIdAsync(name);
            }
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            if (context.Command is CreateApp createApp)
            {
                var index = Index();

                var token = await CheckAppAsync(index, createApp);

                try
                {
                    await next(context);
                }
                finally
                {
                    if (token != null)
                    {
                        if (context.IsCompleted)
                        {
                            await index.AddAsync(token);

                            await Index(createApp.Actor.Identifier).AddAsync(createApp.AppId);
                        }
                        else
                        {
                            await index.RemoveReservationAsync(token);
                        }
                    }
                }
            }
            else
            {
                await next(context);

                if (context.IsCompleted && context.Command is AppCommand appCommand)
                {
                    var app = context.PlainResult as IAppEntity;

                    if (app == null)
                    {
                        app = await GetAppCoreAsync(appCommand.AggregateId, true);
                    }

                    if (app != null)
                    {
                        await InvalidateItAsync(app);

                        switch (context.Command)
                        {
                            case AssignContributor assignContributor:
                                await AssignContributorAsync(assignContributor);
                                break;

                            case RemoveContributor removeContributor:
                                await RemoveContributorAsync(removeContributor);
                                break;

                            case ArchiveApp:
                                await ArchiveAppAsync(app);
                                break;
                        }
                    }
                }
            }
        }

        private static async Task<string?> CheckAppAsync(IAppsByNameIndexGrain index, CreateApp command)
        {
            var name = command.Name;

            if (name.IsSlug())
            {
                var token = await index.ReserveAsync(command.AppId, name);

                if (token == null)
                {
                    throw new ValidationException(T.Get("apps.nameAlreadyExists"));
                }

                return token;
            }

            return null;
        }

        private async Task AssignContributorAsync(AssignContributor command)
        {
            await Index(command.ContributorId).AddAsync(command.AggregateId);
        }

        private async Task RemoveContributorAsync(RemoveContributor command)
        {
            await Index(command.ContributorId).RemoveAsync(command.AggregateId);
        }

        private async Task ArchiveAppAsync(IAppEntity app)
        {
            await Index().RemoveAsync(app.Id);

            foreach (var contributorId in app.Contributors.Keys)
            {
                await Index(contributorId).RemoveAsync(app.Id);
            }

            if (app.CreatedBy.IsClient || !app.Contributors.ContainsKey(app.CreatedBy.Identifier))
            {
                await Index(app.CreatedBy.Identifier).RemoveAsync(app.Id);
            }
        }

        private IAppsByNameIndexGrain Index()
        {
            return grainFactory.GetGrain<IAppsByNameIndexGrain>(SingleGrain.Id);
        }

        private IAppsByUserIndexGrain Index(string id)
        {
            return grainFactory.GetGrain<IAppsByUserIndexGrain>(id);
        }

        private async Task<IAppEntity?> GetAppCoreAsync(DomainId id, bool allowArchived = false)
        {
            var app = (await grainFactory.GetGrain<IAppGrain>(id.ToString()).GetStateAsync()).Value;

            if (app.Version <= EtagVersion.Empty || (app.IsArchived && !allowArchived))
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

        private Task InvalidateItAsync(IAppEntity app)
        {
            return grainCache.RemoveAsync(
                GetCacheKey(app.Id),
                GetCacheKey(app.Name));
        }

        private Task CacheItAsync(IAppEntity app)
        {
            return Task.WhenAll(
                grainCache.AddAsync(GetCacheKey(app.Id), app, CacheDuration),
                grainCache.AddAsync(GetCacheKey(app.Name), app, CacheDuration));
        }
    }
}
