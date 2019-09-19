// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Tasks;

namespace Migrate_01.Migrations
{
    public class PopulateGrainIndexes : IMigration
    {
        private readonly IAppsIndex indexApps;
        private readonly IRulesIndex indexRules;
        private readonly ISchemasIndex indexSchemas;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly IEventStore eventStore;

        public PopulateGrainIndexes(
            IAppsIndex indexApps,
            IRulesIndex indexRules,
            ISchemasIndex indexSchemas,
            IEventDataFormatter eventDataFormatter,
            IEventStore eventStore)
        {
            this.indexApps = indexApps;
            this.indexRules = indexRules;
            this.indexSchemas = indexSchemas;
            this.eventDataFormatter = eventDataFormatter;
            this.eventStore = eventStore;
        }

        public async Task UpdateAsync()
        {
            await Task.WhenAll(
                RebuildAppIndexes(),
                RebuildRuleIndexes(),
                RebuildSchemaIndexes());
        }

        private async Task RebuildAppIndexes()
        {
            var appsByName = new Dictionary<string, Guid>();
            var appsByUser = new Dictionary<string, HashSet<Guid>>();

            await eventStore.QueryAsync(storedEvent =>
            {
                var @event = eventDataFormatter.Parse(storedEvent.Data);

                switch (@event.Payload)
                {
                    case AppCreated appCreated:
                        appsByName[appCreated.Name] = appCreated.AppId.Id;
                        break;
                    case AppContributorAssigned appContributorAssigned:
                        appsByUser.GetOrAddNew(appContributorAssigned.ContributorId).Add(appContributorAssigned.AppId.Id);
                        break;
                    case AppContributorRemoved contributorRemoved:
                        appsByUser.GetOrAddNew(contributorRemoved.ContributorId).Remove(contributorRemoved.AppId.Id);
                        break;
                    case AppArchived appArchived:
                        {
                            foreach (var apps in appsByUser.Values)
                            {
                                apps.Remove(appArchived.AppId.Id);
                            }

                            appsByName.Remove(appArchived.AppId.Name);
                            break;
                        }
                }

                return TaskHelper.Done;
            }, "^app\\-");

            await indexApps.RebuildAsync(appsByName);

            foreach (var kvp in appsByUser)
            {
                await indexApps.RebuildByContributorsAsync(kvp.Key, kvp.Value);
            }
        }

        private async Task RebuildRuleIndexes()
        {
            var rulesByApp = new Dictionary<Guid, HashSet<Guid>>();

            await eventStore.QueryAsync(storedEvent =>
            {
                var @event = eventDataFormatter.Parse(storedEvent.Data);

                switch (@event.Payload)
                {
                    case RuleCreated ruleCreated:
                        rulesByApp.GetOrAddNew(ruleCreated.AppId.Id).Add(ruleCreated.RuleId);
                        break;
                    case RuleDeleted ruleDeleted:
                        rulesByApp.GetOrAddNew(ruleDeleted.AppId.Id).Remove(ruleDeleted.RuleId);
                        break;
                }

                return TaskHelper.Done;
            }, "^rule\\-");

            foreach (var kvp in rulesByApp)
            {
                await indexRules.RebuildAsync(kvp.Key, kvp.Value);
            }
        }

        private async Task RebuildSchemaIndexes()
        {
            var schemasByApp = new Dictionary<Guid, Dictionary<string, Guid>>();

            await eventStore.QueryAsync(storedEvent =>
            {
                var @event = eventDataFormatter.Parse(storedEvent.Data);

                switch (@event.Payload)
                {
                    case SchemaCreated schemaCreated:
                        schemasByApp.GetOrAddNew(schemaCreated.AppId.Id)[schemaCreated.SchemaId.Name] = schemaCreated.SchemaId.Id;
                        break;
                    case SchemaDeleted schemaDeleted:
                        schemasByApp.GetOrAddNew(schemaDeleted.AppId.Id).Remove(schemaDeleted.SchemaId.Name);
                        break;
                }

                return TaskHelper.Done;
            }, "^schema\\-");

            foreach (var kvp in schemasByApp)
            {
                await indexSchemas.RebuildAsync(kvp.Key, kvp.Value);
            }
        }
    }
}