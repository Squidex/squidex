// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities
{
    public sealed class AppProvider : IAppProvider
    {
        private readonly IGrainFactory grainFactory;
        private readonly IAppRepository appRepository;
        private readonly IRuleRepository ruleRepository;
        private readonly ISchemaRepository schemaRepository;

        public AppProvider(
            IGrainFactory grainFactory,
            IAppRepository appRepository,
            ISchemaRepository schemaRepository,
            IRuleRepository ruleRepository)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));
            Guard.NotNull(appRepository, nameof(appRepository));
            Guard.NotNull(schemaRepository, nameof(schemaRepository));
            Guard.NotNull(ruleRepository, nameof(ruleRepository));

            this.grainFactory = grainFactory;
            this.appRepository = appRepository;
            this.schemaRepository = schemaRepository;
            this.ruleRepository = ruleRepository;
        }

        public async Task<(IAppEntity, ISchemaEntity)> GetAppWithSchemaAsync(Guid appId, Guid id)
        {
            var app = await grainFactory.GetGrain<IAppGrain>(appId).GetStateAsync();

            if (!IsFound(app.Value))
            {
                return (null, null);
            }

            var schema = await grainFactory.GetGrain<ISchemaGrain>(id).GetStateAsync();

            if (!IsFound(schema.Value) || schema.Value.IsDeleted)
            {
                return (null, null);
            }

            return (app.Value, schema.Value);
        }

        public async Task<IAppEntity> GetAppAsync(string appName)
        {
            var appId = await GetAppIdAsync(appName);

            if (appId == Guid.Empty)
            {
                return null;
            }

            return (await grainFactory.GetGrain<IAppGrain>(appId).GetStateAsync()).Value;
        }

        public async Task<ISchemaEntity> GetSchemaAsync(Guid appId, string name)
        {
            var schemaId = await GetSchemaIdAsync(appId, name);

            if (schemaId == Guid.Empty)
            {
                return null;
            }

            return (await grainFactory.GetGrain<ISchemaGrain>(schemaId).GetStateAsync()).Value;
        }

        public async Task<ISchemaEntity> GetSchemaAsync(Guid appId, Guid id, bool allowDeleted = false)
        {
            var schema = await grainFactory.GetGrain<ISchemaGrain>(id).GetStateAsync();

            if (!IsFound(schema.Value) || (schema.Value.IsDeleted && !allowDeleted) || schema.Value.AppId.Id != appId)
            {
                return null;
            }

            return schema.Value;
        }

        public async Task<List<ISchemaEntity>> GetSchemasAsync(Guid appId)
        {
            var ids = await schemaRepository.QuerySchemaIdsAsync(appId);

            var schemas =
                await Task.WhenAll(
                    ids.Select(id => grainFactory.GetGrain<ISchemaGrain>(id).GetStateAsync()));

            return schemas.Where(s => IsFound(s.Value)).Select(s => (ISchemaEntity)s.Value).ToList();
        }

        public async Task<List<IRuleEntity>> GetRulesAsync(Guid appId)
        {
            var ids = await ruleRepository.QueryRuleIdsAsync(appId);

            var rules =
                await Task.WhenAll(
                    ids.Select(id => grainFactory.GetGrain<IRuleGrain>(id).GetStateAsync()));

            return rules.Where(r => IsFound(r.Value)).Select(r => (IRuleEntity)r.Value).ToList();
        }

        public async Task<List<IAppEntity>> GetUserApps(string userId)
        {
            var ids = await appRepository.QueryUserAppIdsAsync(userId);

            var apps =
                await Task.WhenAll(
                    ids.Select(id => grainFactory.GetGrain<IAppGrain>(id).GetStateAsync()));

            return apps.Where(a => IsFound(a.Value)).Select(a => (IAppEntity)a.Value).ToList();
        }

        private Task<Guid> GetAppIdAsync(string name)
        {
            return appRepository.FindAppIdByNameAsync(name);
        }

        private async Task<Guid> GetSchemaIdAsync(Guid appId, string name)
        {
            return await schemaRepository.FindSchemaIdAsync(appId, name);
        }

        private static bool IsFound(IEntityWithVersion entity)
        {
            return entity.Version > EtagVersion.Empty;
        }
    }
}
