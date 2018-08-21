// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.Rules.State;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Migrate_01.Migrations
{
    public class PopulateGrainIndexes : IMigration
    {
        private readonly IGrainFactory grainFactory;
        private readonly ISnapshotStore<AppState, Guid> statesForApps;
        private readonly ISnapshotStore<RuleState, Guid> statesForRules;
        private readonly ISnapshotStore<SchemaState, Guid> statesForSchemas;

        public PopulateGrainIndexes(
            IGrainFactory grainFactory,
            ISnapshotStore<AppState, Guid> statesForApps,
            ISnapshotStore<RuleState, Guid> statesForRules,
            ISnapshotStore<SchemaState, Guid> statesForSchemas)
        {
            this.grainFactory = grainFactory;
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

            await grainFactory.GetGrain<IAppsByNameIndex>(SingleGrain.Id).RebuildAsync(appsByName);

            foreach (var kvp in appsByUser)
            {
                await grainFactory.GetGrain<IAppsByUserIndex>(kvp.Key).RebuildAsync(kvp.Value);
            }
        }

        private async Task RebuildRuleIndexes()
        {
            var rulesByApp = new Dictionary<Guid, HashSet<Guid>>();

            await statesForRules.ReadAllAsync((schema, version) =>
            {
                if (!schema.IsDeleted)
                {
                    rulesByApp.GetOrAddNew(schema.AppId.Id).Add(schema.Id);
                }

                return TaskHelper.Done;
            });

            foreach (var kvp in rulesByApp)
            {
                await grainFactory.GetGrain<IRulesByAppIndex>(kvp.Key).RebuildAsync(kvp.Value);
            }
        }

        private async Task RebuildSchemaIndexes()
        {
            var schemasByApp = new Dictionary<Guid, Dictionary<string, Guid>>();

            await statesForSchemas.ReadAllAsync((schema, version) =>
            {
                if (!schema.IsDeleted)
                {
                    schemasByApp.GetOrAddNew(schema.AppId.Id)[schema.Name] = schema.Id;
                }

                return TaskHelper.Done;
            });

            foreach (var kvp in schemasByApp)
            {
                await grainFactory.GetGrain<ISchemasByAppIndex>(kvp.Key).RebuildAsync(kvp.Value);
            }
        }
    }
}
