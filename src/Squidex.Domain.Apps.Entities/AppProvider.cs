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
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities
{
    public sealed class AppProvider : IAppProvider
    {
        private readonly IAppRepository appRepository;
        private readonly IRuleRepository ruleRepository;
        private readonly ISchemaRepository schemaRepository;
        private readonly IStateFactory stateFactory;

        public AppProvider(
            IAppRepository appRepository,
            ISchemaRepository schemaRepository,
            IStateFactory stateFactory,
            IRuleRepository ruleRepository)
        {
            Guard.NotNull(appRepository, nameof(appRepository));
            Guard.NotNull(schemaRepository, nameof(schemaRepository));
            Guard.NotNull(stateFactory, nameof(stateFactory));
            Guard.NotNull(ruleRepository, nameof(ruleRepository));

            this.appRepository = appRepository;
            this.schemaRepository = schemaRepository;
            this.stateFactory = stateFactory;
            this.ruleRepository = ruleRepository;
        }

        public async Task<(IAppEntity, ISchemaEntity)> GetAppWithSchemaAsync(Guid appId, Guid id)
        {
            var app = await stateFactory.GetSingleAsync<AppGrain>(appId);

            if (!IsFound(app))
            {
                return (null, null);
            }

            var schema = await stateFactory.GetSingleAsync<SchemaGrain>(id);

            if (!IsFound(schema) || schema.Snapshot.IsDeleted)
            {
                return (null, null);
            }

            return (app.Snapshot, schema.Snapshot);
        }

        public async Task<IAppEntity> GetAppAsync(string appName)
        {
            var appId = await GetAppIdAsync(appName);

            if (appId == Guid.Empty)
            {
                return null;
            }

            return (await stateFactory.GetSingleAsync<AppGrain>(appId)).Snapshot;
        }

        public async Task<ISchemaEntity> GetSchemaAsync(Guid appId, string name)
        {
            var schemaId = await GetSchemaIdAsync(appId, name);

            if (schemaId == Guid.Empty)
            {
                return null;
            }

            return (await stateFactory.GetSingleAsync<SchemaGrain>(schemaId)).Snapshot;
        }

        public async Task<ISchemaEntity> GetSchemaAsync(Guid appId, Guid id, bool allowDeleted = false)
        {
            var schema = await stateFactory.GetSingleAsync<SchemaGrain>(id);

            if (!IsFound(schema) || (schema.Snapshot.IsDeleted && !allowDeleted) || schema.Snapshot.AppId.Id != appId)
            {
                return null;
            }

            return schema.Snapshot;
        }

        public async Task<List<ISchemaEntity>> GetSchemasAsync(Guid appId)
        {
            var ids = await schemaRepository.QuerySchemaIdsAsync(appId);

            var schemas =
                await Task.WhenAll(
                    ids.Select(id => stateFactory.GetSingleAsync<SchemaGrain>(id)));

            return schemas.Where(IsFound).Select(s => (ISchemaEntity)s.Snapshot).ToList();
        }

        public async Task<List<IRuleEntity>> GetRulesAsync(Guid appId)
        {
            var ids = await ruleRepository.QueryRuleIdsAsync(appId);

            var rules =
                await Task.WhenAll(
                    ids.Select(id => stateFactory.GetSingleAsync<RuleGrain>(id)));

            return rules.Where(IsFound).Select(r => (IRuleEntity)r.Snapshot).ToList();
        }

        public async Task<List<IAppEntity>> GetUserApps(string userId)
        {
            var ids = await appRepository.QueryUserAppIdsAsync(userId);

            var apps =
                await Task.WhenAll(
                    ids.Select(id => stateFactory.GetSingleAsync<AppGrain>(id)));

            return apps.Where(IsFound).Select(a => (IAppEntity)a.Snapshot).ToList();
        }

        private Task<Guid> GetAppIdAsync(string name)
        {
            return appRepository.FindAppIdByNameAsync(name);
        }

        private async Task<Guid> GetSchemaIdAsync(Guid appId, string name)
        {
            return await schemaRepository.FindSchemaIdAsync(appId, name);
        }

        private static bool IsFound(IDomainObjectGrain app)
        {
            return app.Version > EtagVersion.Empty;
        }
    }
}
