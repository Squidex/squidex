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
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.State;
using Squidex.Domain.Apps.Entities.Schemas;
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

            var tasks =
                appsByUser.Select(x =>
                    grainFactory.GetGrain<IAppsByUserIndex>(x.Key).RebuildAsync(x.Value))
                .Union(new[]
                {
                    grainFactory.GetGrain<IAppsByNameIndex>(SingleGrain.Id).RebuildAsync(appsByName)
                });

            await Task.WhenAll(tasks);
        }

        private async Task RebuildRuleIndexes()
        {
            var schemasByApp = new Dictionary<Guid, HashSet<Guid>>();

            await statesForRules.ReadAllAsync((schema, version) =>
            {
                if (!schema.IsDeleted)
                {
                    schemasByApp.GetOrAddNew(schema.AppId.Id).Add(schema.Id);
                }

                return TaskHelper.Done;
            });

            var tasks =
                schemasByApp.Select(x =>
                    grainFactory.GetGrain<IRulesByAppIndex>(x.Key).RebuildAsync(x.Value));

            await Task.WhenAll(tasks);
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

            var tasks =
                schemasByApp.Select(x =>
                    grainFactory.GetGrain<ISchemasByAppIndex>(x.Key).RebuildAsync(x.Value));

            await Task.WhenAll(tasks);
        }
    }
}
