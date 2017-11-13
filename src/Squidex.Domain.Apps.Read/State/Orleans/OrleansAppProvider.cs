// ==========================================================================
//  OrleansApps.cs
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
using Squidex.Domain.Apps.Read.State.Orleans.Grains;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read.State.Orleans
{
    public sealed class OrleansAppProvider : IAppProvider
    {
        private readonly IGrainFactory factory;

        public OrleansAppProvider(IGrainFactory factory)
        {
            Guard.NotNull(factory, nameof(factory));

            this.factory = factory;
        }

        public async Task<IAppEntity> GetAppAsync(string appName)
        {
            var result = await factory.GetGrain<IAppStateGrain>(appName).GetAppAsync();

            return result.Value;
        }

        public async Task<(IAppEntity, ISchemaEntity)> GetAppWithSchemaAsync(string appName, Guid id)
        {
            var result = await factory.GetGrain<IAppStateGrain>(appName).GetAppWithSchemaAsync(id);

            return result.Value;
        }

        public async Task<List<IRuleEntity>> GetRulesAsync(string appName)
        {
            var result = await factory.GetGrain<IAppStateGrain>(appName).GetRulesAsync();

            return result.Value;
        }

        public async Task<ISchemaEntity> GetSchemaAsync(string appName, Guid id, bool provideDeleted = false)
        {
            var result = await factory.GetGrain<IAppStateGrain>(appName).GetSchemaAsync(id, provideDeleted);

            return result.Value;
        }

        public async Task<ISchemaEntity> GetSchemaAsync(string appName, string name, bool provideDeleted = false)
        {
            var result = await factory.GetGrain<IAppStateGrain>(appName).GetSchemaAsync(name, provideDeleted);

            return result.Value;
        }

        public async Task<List<ISchemaEntity>> GetSchemasAsync(string appName)
        {
            var result = await factory.GetGrain<IAppStateGrain>(appName).GetSchemasAsync();

            return result.Value;
        }

        public async Task<List<IAppEntity>> GetUserApps(string userId)
        {
            var schemaIds = await factory.GetGrain<IAppUserGrain>(userId).GetSchemaNamesAsync();

            var tasks =
                schemaIds
                    .Select(x => factory.GetGrain<IAppStateGrain>(x))
                    .Select(x => x.GetAppAsync());

            var apps = await Task.WhenAll(tasks);

            return apps.Select(a => a.Value).Where(a => a != null).ToList();
        }
    }
}
