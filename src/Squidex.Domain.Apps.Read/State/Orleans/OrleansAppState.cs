// ==========================================================================
//  OrleansAppState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains
{
    public sealed class OrleansAppState : IAppState
    {
        private readonly IGrainFactory factory;

        public OrleansAppState(IGrainFactory factory)
        {
            Guard.NotNull(factory, nameof(factory));

            this.factory = factory;
        }

        public Task<IAppEntity> GetAppAsync(Guid appId)
        {
            return factory.GetGrain<IAppStateGrain>(appId).GetAppAsync();
        }

        public Task<List<IRuleEntity>> GetRulesAsync(Guid appId)
        {
            return factory.GetGrain<IAppStateGrain>(appId).GetRulesAsync();
        }

        public Task<ISchemaEntity> GetSchemaAsync(Guid appId, Guid id, bool provideDeleted = false)
        {
            return factory.GetGrain<IAppStateGrain>(appId).GetSchemaAsync(id, provideDeleted);
        }

        public Task<ISchemaEntity> GetSchemaAsync(Guid appId, string name, bool provideDeleted = false)
        {
            return factory.GetGrain<IAppStateGrain>(appId).GetSchemaAsync(name, provideDeleted);
        }

        public Task<List<ISchemaEntity>> GetSchemasAsync(Guid appId)
        {
            return factory.GetGrain<IAppStateGrain>(appId).GetSchemasAsync();
        }

        public async Task<List<IAppEntity>> GetUserApps(string userId)
        {
            var schemaIds = await factory.GetGrain<IAppUserGrain>(userId).GetSchemaIdsAsync();

            var tasks =
                schemaIds
                    .Select(x => factory.GetGrain<IAppStateGrain>(x))
                    .Select(x => x.GetAppAsync());

            var apps = await Task.WhenAll(tasks);

            return apps.Where(a => a != null).ToList();
        }
    }
}
