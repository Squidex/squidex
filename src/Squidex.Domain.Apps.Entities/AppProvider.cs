// ==========================================================================
//  AppProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;
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
            var app = await stateFactory.GetSingleAsync<AppDomainObject>(appId);

            if (IsNotFound(app))
            {
                return (null, null);
            }

            var schema = await stateFactory.GetSingleAsync<SchemaDomainObject>(id);

            return IsNotFound(false, schema) ? (null, null) : (app.State, schema.State);
        }

        public async Task<IAppEntity> GetAppAsync(string appName)
        {
            var appId = await GetAppIdAsync(appName);

            var app = await stateFactory.GetSingleAsync<AppDomainObject>(appId);

            return IsNotFound(app) ? null : app.State;
        }

        public async Task<ISchemaEntity> GetSchemaAsync(Guid appId, string name, bool provideDeleted = false)
        {
            var schemaId = await GetSchemaIdAsync(appId, name);

            var schema = await stateFactory.GetSingleAsync<SchemaDomainObject>(schemaId);

            return IsNotFound(provideDeleted, schema) ? null : schema.State;
        }

        public async Task<ISchemaEntity> GetSchemaAsync(Guid appId, Guid id, bool provideDeleted = false)
        {
            var schema = await stateFactory.GetSingleAsync<SchemaDomainObject>(id);

            return IsNotFound(provideDeleted, schema) ? null : schema.State;
        }

        public async Task<List<ISchemaEntity>> GetSchemasAsync(Guid appId)
        {
            var ids = await schemaRepository.QuerySchemaIdsAsync(appId);

            var schemas =
                await Task.WhenAll(
                    ids.Select(id => stateFactory.GetSingleAsync<SchemaDomainObject>(id)));

            return schemas.Select(a => (ISchemaEntity)a.State).ToList();
        }

        public async Task<List<IRuleEntity>> GetRulesAsync(Guid appId)
        {
            var ids = await ruleRepository.QueryRuleIdsAsync(appId);

            var rules =
                await Task.WhenAll(
                    ids.Select(id => stateFactory.GetSingleAsync<RuleDomainObject>(id)));

            return rules.Select(a => (IRuleEntity)a.State).ToList();
        }

        public async Task<List<IAppEntity>> GetUserApps(string userId)
        {
            var ids = await appRepository.QueryUserAppIdsAsync(userId);

            var apps =
                await Task.WhenAll(
                    ids.Select(id => stateFactory.GetSingleAsync<AppDomainObject>(id)));

            return apps.Select(a => (IAppEntity)a.State).ToList();
        }

        private Task<Guid> GetAppIdAsync(string name)
        {
            return appRepository.FindAppIdByNameAsync(name);
        }

        private Task<Guid> GetSchemaIdAsync(Guid appId, string name)
        {
            return schemaRepository.FindSchemaIdAsync(appId, name);
        }

        private static bool IsNotFound(AppDomainObject app)
        {
            return app.Version < 0;
        }

        private static bool IsNotFound(bool provideDeleted, SchemaDomainObject schema)
        {
            return schema.Version < 0 || (schema.State.IsDeleted && !provideDeleted);
        }
    }
}
