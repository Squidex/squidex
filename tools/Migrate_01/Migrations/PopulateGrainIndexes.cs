﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.Rules.State;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Migrate_01.Migrations
{
    public class PopulateGrainIndexes : IMigration
    {
        private readonly IAppsIndex indexApps;
        private readonly IRulesIndex indexRules;
        private readonly ISchemasIndex indexSchemas;
        private readonly ISnapshotStore<AppState, Guid> statesForApps;
        private readonly ISnapshotStore<RuleState, Guid> statesForRules;
        private readonly ISnapshotStore<SchemaState, Guid> statesForSchemas;

        public PopulateGrainIndexes(
            IAppsIndex indexApps,
            IRulesIndex indexRules,
            ISchemasIndex indexSchemas,
            ISnapshotStore<AppState, Guid> statesForApps,
            ISnapshotStore<RuleState, Guid> statesForRules,
            ISnapshotStore<SchemaState, Guid> statesForSchemas)
        {
            this.indexApps = indexApps;
            this.indexRules = indexRules;
            this.indexSchemas = indexSchemas;
            this.statesForApps = statesForApps;
            this.statesForRules = statesForRules;
            this.statesForSchemas = statesForSchemas;
        }

        public Task UpdateAsync()
        {
            return Task.WhenAll(
                RebuildAppIndexes(),
                RebuildRuleIndexes(),
                RebuildSchemaIndexes());
        }

        private async Task RebuildAppIndexes()
        {
            var appsByName = new Dictionary<string, Guid>();
            var appsByUser = new Dictionary<string, HashSet<Guid>>();

            await statesForApps.ReadAllAsync((app, version) =>
            {
                if (!app.IsArchived)
                {
                    appsByName[app.Name] = app.Id;

                    foreach (var contributor in app.Contributors.Keys)
                    {
                        appsByUser.GetOrAddNew(contributor).Add(app.Id);
                    }
                }

                return TaskHelper.Done;
            });

            await indexApps.RebuildAsync(appsByName);

            foreach (var kvp in appsByUser)
            {
                await indexApps.RebuildByContributorsAsync(kvp.Key, kvp.Value);
            }
        }

        private async Task RebuildRuleIndexes()
        {
            var rulesByApp = new Dictionary<Guid, HashSet<Guid>>();

            await statesForRules.ReadAllAsync((rule, version) =>
            {
                if (!rule.IsDeleted)
                {
                    rulesByApp.GetOrAddNew(rule.AppId.Id).Add(rule.Id);
                }

                return TaskHelper.Done;
            });

            foreach (var kvp in rulesByApp)
            {
                await indexRules.RebuildAsync(kvp.Key, kvp.Value);
            }
        }

        private async Task RebuildSchemaIndexes()
        {
            var schemasByApp = new Dictionary<Guid, Dictionary<string, Guid>>();

            await statesForSchemas.ReadAllAsync((schema, version) =>
            {
                if (!schema.IsDeleted)
                {
                    schemasByApp.GetOrAddNew(schema.AppId.Id)[schema.SchemaDef.Name] = schema.Id;
                }

                return TaskHelper.Done;
            });

            foreach (var kvp in schemasByApp)
            {
                await indexSchemas.RebuildAsync(kvp.Key, kvp.Value);
            }
        }
    }
}
