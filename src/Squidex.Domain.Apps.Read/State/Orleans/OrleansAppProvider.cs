// ==========================================================================
//  OrleansAppProvider.cs
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
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Read.State.Orleans
{
    public sealed class OrleansAppProvider : IAppProvider
    {
        private readonly IGrainFactory factory;
        private readonly ISemanticLog log;

        public OrleansAppProvider(IGrainFactory factory, ISemanticLog log)
        {
            Guard.NotNull(factory, nameof(factory));
            Guard.NotNull(log, nameof(log));

            this.factory = factory;

            this.log = log;
        }

        public async Task<IAppEntity> GetAppAsync(string appName)
        {
            using (log.MeasureTrace(w => w
                .WriteProperty("module", nameof(OrleansAppProvider))
                .WriteProperty("method", nameof(GetAppAsync))))
            {
                var result = await factory.GetGrain<IAppStateGrain>(appName).GetAppAsync();

                return result.Value;
            }
        }

        public async Task<(IAppEntity, ISchemaEntity)> GetAppWithSchemaAsync(string appName, Guid id)
        {
            using (log.MeasureTrace(w => w
                .WriteProperty("module", nameof(OrleansAppProvider))
                .WriteProperty("method", nameof(GetAppWithSchemaAsync))))
            {
                var result = await factory.GetGrain<IAppStateGrain>(appName).GetAppWithSchemaAsync(id);

                return result.Value;
            }
        }

        public async Task<List<IRuleEntity>> GetRulesAsync(string appName)
        {
            using (log.MeasureTrace(w => w
                .WriteProperty("module", nameof(OrleansAppProvider))
                .WriteProperty("method", nameof(GetRulesAsync))))
            {
                var result = await factory.GetGrain<IAppStateGrain>(appName).GetRulesAsync();

                return result.Value;
            }
        }

        public async Task<ISchemaEntity> GetSchemaAsync(string appName, Guid id, bool provideDeleted = false)
        {
            using (log.MeasureTrace(w => w
                .WriteProperty("module", nameof(OrleansAppProvider))
                .WriteProperty("method", nameof(GetSchemaAsync))))
            {
                var result = await factory.GetGrain<IAppStateGrain>(appName).GetSchemaAsync(id, provideDeleted);

                return result.Value;
            }
        }

        public async Task<ISchemaEntity> GetSchemaAsync(string appName, string name, bool provideDeleted = false)
        {
            using (log.MeasureTrace(w => w
                .WriteProperty("module", nameof(OrleansAppProvider))
                .WriteProperty("method", nameof(GetSchemaAsync))))
            {
                var result = await factory.GetGrain<IAppStateGrain>(appName).GetSchemaAsync(name, provideDeleted);

                return result.Value;
            }
        }

        public async Task<List<ISchemaEntity>> GetSchemasAsync(string appName)
        {
            using (log.MeasureTrace(w => w
                .WriteProperty("module", nameof(OrleansAppProvider))
                .WriteProperty("method", nameof(GetSchemasAsync))))
            {
                var result = await factory.GetGrain<IAppStateGrain>(appName).GetSchemasAsync();

                return result.Value;
            }
        }

        public async Task<List<IAppEntity>> GetUserApps(string userId)
        {
            using (log.MeasureTrace(w => w
                .WriteProperty("module", nameof(OrleansAppProvider))
                .WriteProperty("method", nameof(GetUserApps))))
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
}
