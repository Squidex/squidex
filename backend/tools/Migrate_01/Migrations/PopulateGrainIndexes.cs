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
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Domain.Apps.Events;
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

        public PopulateGrainIndexes(IAppsIndex indexApps, IRulesIndex indexRules, ISchemasIndex indexSchemas,
            IEventDataFormatter eventDataFormatter,
            IEventStore eventStore)
        {
            this.indexApps = indexApps;
            this.indexRules = indexRules;
            this.indexSchemas = indexSchemas;
            this.eventDataFormatter = eventDataFormatter;
            this.eventStore = eventStore;
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

            bool HasApp(NamedId<Guid> appId, bool consistent, out Guid id)
            {
                return appsByName.TryGetValue(appId.Name, out id) && (!consistent || id == appId.Id);
            }

            HashSet<Guid> Index(string contributorId)
            {
                return appsByUser.GetOrAddNew(contributorId);
            }

            void RemoveApp(NamedId<Guid> appId, bool consistent)
            {
                if (HasApp(appId, consistent, out var id))
                {
                    foreach (var apps in appsByUser.Values)
                    {
                        apps.Remove(id);
                    }

                    appsByName.Remove(appId.Name);
                }
            }

            await eventStore.QueryAsync(storedEvent =>
            {
                var @event = eventDataFormatter.Parse(storedEvent.Data);

                switch (@event.Payload)
                {
                    case AppCreated created:
                        {
                            RemoveApp(created.AppId, false);

                            appsByName[created.Name] = created.AppId.Id;
                            break;
                        }

                    case AppContributorAssigned contributorAssigned:
                        {
                            if (HasApp(contributorAssigned.AppId, true, out _))
                            {
                                Index(contributorAssigned.ContributorId).Add(contributorAssigned.AppId.Id);
                            }

                            break;
                        }

                    case AppContributorRemoved contributorRemoved:
                        Index(contributorRemoved.ContributorId).Remove(contributorRemoved.AppId.Id);
                        break;
                    case AppArchived archived:
                        RemoveApp(archived.AppId, true);
                        break;
                }

                return TaskHelper.Done;
            }, "^app\\-");

            await indexApps.RebuildAsync(appsByName);

            foreach (var (contributorId, apps) in appsByUser)
            {
                await indexApps.RebuildByContributorsAsync(contributorId, apps);
            }
        }

        private async Task RebuildRuleIndexes()
        {
            var rulesByApp = new Dictionary<Guid, HashSet<Guid>>();

            HashSet<Guid> Index(RuleEvent @event)
            {
                return rulesByApp.GetOrAddNew(@event.AppId.Id);
            }

            await eventStore.QueryAsync(storedEvent =>
            {
                var @event = eventDataFormatter.Parse(storedEvent.Data);

                switch (@event.Payload)
                {
                    case RuleCreated created:
                        Index(created).Add(created.RuleId);
                        break;
                    case RuleDeleted deleted:
                        Index(deleted).Remove(deleted.RuleId);
                        break;
                }

                return TaskHelper.Done;
            }, "^rule\\-");

            foreach (var (appId, rules) in rulesByApp)
            {
                await indexRules.RebuildAsync(appId, rules);
            }
        }

        private async Task RebuildSchemaIndexes()
        {
            var schemasByApp = new Dictionary<Guid, Dictionary<string, Guid>>();

            Dictionary<string, Guid> Index(SchemaEvent @event)
            {
                return schemasByApp.GetOrAddNew(@event.AppId.Id);
            }

            await eventStore.QueryAsync(storedEvent =>
            {
                var @event = eventDataFormatter.Parse(storedEvent.Data);

                switch (@event.Payload)
                {
                    case SchemaCreated created:
                        Index(created)[created.SchemaId.Name] = created.SchemaId.Id;
                        break;
                    case SchemaDeleted deleted:
                        Index(deleted).Remove(deleted.SchemaId.Name);
                        break;
                }

                return TaskHelper.Done;
            }, "^schema\\-");

            foreach (var (appId, schemas) in schemasByApp)
            {
                await indexSchemas.RebuildAsync(appId, schemas);
            }
        }
    }
}