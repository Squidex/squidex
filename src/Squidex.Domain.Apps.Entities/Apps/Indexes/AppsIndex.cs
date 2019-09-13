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
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Validation;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsIndex : IAppsIndex, ICommandMiddleware
    {
        private readonly IGrainFactory grainFactory;

        public AppsIndex(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public async Task RebuildByContributorsAsync(Guid appId, HashSet<string> contributors)
        {
            foreach (var contributorId in contributors)
            {
                await Index(contributorId).AddAsync(appId);
            }
        }

        public Task RebuildByContributorsAsync(string contributorId, HashSet<Guid> apps)
        {
            return Index(contributorId).RebuildAsync(apps);
        }

        public Task RebuildAsync(Dictionary<string, Guid> appsByName)
        {
            return Index().RebuildAsync(appsByName);
        }

        public Task RemoveReservationAsync(string token)
        {
            return Index().RemoveReservationAsync(token);
        }

        public Task<bool> AddAsync(string token)
        {
            return Index().AddAsync(token);
        }

        public Task<string> ReserveAsync(Guid id, string name)
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
                        .Select(id => GetAppAsync(id)));

                return apps.Where(x => x != null).ToList();
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
                        .SelectMany(x => x)
                        .Select(id => GetAppAsync(id)));

                return apps.Where(x => x != null).ToList();
            }
        }

        public async Task<IAppEntity> GetAppAsync(string name)
        {
            using (Profiler.TraceMethod<AppsIndex>())
            {
                var appId = await GetAppIdAsync(name);

                if (appId == default)
                {
                    return null;
                }

                return await GetAppAsync(appId);
            }
        }

        public async Task<IAppEntity> GetAppAsync(Guid appId)
        {
            using (Profiler.TraceMethod<AppsIndex>())
            {
                var app = await grainFactory.GetGrain<IAppGrain>(appId).GetStateAsync();

                if (IsFound(app.Value))
                {
                    return app.Value;
                }

                await Index().RemoveAsync(appId);

                return null;
            }
        }

        private async Task<List<Guid>> GetAppIdsByUserAsync(string userId)
        {
            using (Profiler.TraceMethod<AppProvider>())
            {
                return await grainFactory.GetGrain<IAppsByUserIndexGrain>(userId).GetIdsAsync();
            }
        }

        private async Task<List<Guid>> GetAppIdsAsync()
        {
            using (Profiler.TraceMethod<AppProvider>())
            {
                return await grainFactory.GetGrain<IAppsByNameIndexGrain>(SingleGrain.Id).GetIdsAsync();
            }
        }

        private async Task<List<Guid>> GetAppIdsAsync(string[] names)
        {
            using (Profiler.TraceMethod<AppProvider>())
            {
                return await grainFactory.GetGrain<IAppsByNameIndexGrain>(SingleGrain.Id).GetIdsAsync(names);
            }
        }

        private async Task<Guid> GetAppIdAsync(string name)
        {
            using (Profiler.TraceMethod<AppProvider>())
            {
                return await grainFactory.GetGrain<IAppsByNameIndexGrain>(SingleGrain.Id).GetIdAsync(name);
            }
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is CreateApp createApp)
            {
                var index = Index();

                string token = await CheckAppAsync(index, createApp);

                try
                {
                    await next();
                }
                finally
                {
                    if (context.IsCompleted)
                    {
                        await index.AddAsync(token);
                    }
                    else
                    {
                        await index.RemoveReservationAsync(token);
                    }
                }
            }
            else
            {
                await next();

                if (context.IsCompleted)
                {
                    switch (context.Command)
                    {
                        case AssignContributor assignContributor:
                            await AssignContributorAsync(assignContributor);
                            break;

                        case RemoveContributor removeContributor:
                            await RemoveContributorAsync(removeContributor);
                            break;

                        case ArchiveApp archiveApp:
                            await ArchiveAppAsync(archiveApp);
                            break;
                    }
                }
            }
        }

        private async Task<string> CheckAppAsync(IAppsByNameIndexGrain index, CreateApp command)
        {
            var name = command.Name;

            if (name.IsSlug())
            {
                var token = await index.ReserveAsync(command.AppId, name);

                if (token == null)
                {
                    var error = new ValidationError("An app with this already exists.");

                    throw new ValidationException("Cannot create app.", error);
                }

                return token;
            }

            return null;
        }

        private Task AssignContributorAsync(AssignContributor command)
        {
            return Index(command.ContributorId).AddAsync(command.AppId);
        }

        private Task RemoveContributorAsync(RemoveContributor command)
        {
            return Index(command.ContributorId).RemoveAsync(command.AppId);
        }

        private async Task ArchiveAppAsync(ArchiveApp command)
        {
            var appId = command.AppId;

            var app = await grainFactory.GetGrain<IAppGrain>(appId).GetStateAsync();

            if (IsFound(app.Value))
            {
                await Index().RemoveAsync(appId);
            }

            foreach (var contributorId in app.Value.Contributors.Keys)
            {
                await Index(contributorId).RemoveAsync(appId);
            }
        }

        private static bool IsFound(IAppEntity app)
        {
            return app.Version > EtagVersion.Empty && !app.IsArchived;
        }

        private IAppsByNameIndexGrain Index()
        {
            return grainFactory.GetGrain<IAppsByNameIndexGrain>(SingleGrain.Id);
        }

        private IAppsByUserIndexGrain Index(string id)
        {
            return grainFactory.GetGrain<IAppsByUserIndexGrain>(id);
        }
    }
}
