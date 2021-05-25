// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
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

namespace Migrations.Migrations
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

        public Task UpdateAsync(CancellationToken ct)
        {
            return Task.WhenAll(
                RebuildAppIndexes(ct),
                RebuildRuleIndexes(ct),
                RebuildSchemaIndexes(ct));
        }

        private async Task RebuildAppIndexes(CancellationToken ct)
        {
            var appsByName = new Dictionary<string, DomainId>();
            var appsByUser = new Dictionary<string, HashSet<DomainId>>();

            bool HasApp(NamedId<DomainId> appId, bool consistent, out DomainId id)
            {
                return appsByName!.TryGetValue(appId.Name, out id) && (!consistent || id == appId.Id);
            }

            HashSet<DomainId> Index(string contributorId)
            {
                return appsByUser!.GetOrAddNew(contributorId);
            }

            void RemoveApp(NamedId<DomainId> appId, bool consistent)
            {
                if (HasApp(appId, consistent, out var id))
                {
                    foreach (var apps in appsByUser!.Values)
                    {
                        apps.Remove(id);
                    }

                    appsByName!.Remove(appId.Name);
                }
            }

            await foreach (var storedEvent in eventStore.QueryAllAsync("^app\\-", ct: ct))
            {
                var @event = eventDataFormatter.ParseIfKnown(storedEvent);

                if (@event != null)
                {
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
                }
            }

            await indexApps.RebuildAsync(appsByName);

            foreach (var (contributorId, apps) in appsByUser)
            {
                await indexApps.RebuildByContributorsAsync(contributorId, apps);
            }
        }

        private async Task RebuildRuleIndexes(CancellationToken ct)
        {
            var rulesByApp = new Dictionary<DomainId, HashSet<DomainId>>();

            HashSet<DomainId> Index(RuleEvent @event)
            {
                return rulesByApp!.GetOrAddNew(@event.AppId.Id);
            }

            await foreach (var storedEvent in eventStore.QueryAllAsync("^rule\\-", ct: ct))
            {
                var @event = eventDataFormatter.ParseIfKnown(storedEvent);

                if (@event != null)
                {
                    switch (@event.Payload)
                    {
                        case RuleCreated created:
                            Index(created).Add(created.RuleId);
                            break;
                        case RuleDeleted deleted:
                            Index(deleted).Remove(deleted.RuleId);
                            break;
                    }
                }
            }

            foreach (var (appId, rules) in rulesByApp)
            {
                await indexRules.RebuildAsync(appId, rules);
            }
        }

        private async Task RebuildSchemaIndexes(CancellationToken ct)
        {
            var schemasByApp = new Dictionary<DomainId, Dictionary<string, DomainId>>();

            Dictionary<string, DomainId> Index(SchemaEvent @event)
            {
                return schemasByApp!.GetOrAddNew(@event.AppId.Id);
            }

            await foreach (var storedEvent in eventStore.QueryAllAsync("^schema\\-", ct: ct))
            {
                var @event = eventDataFormatter.ParseIfKnown(storedEvent);

                if (@event != null)
                {
                    switch (@event.Payload)
                    {
                        case SchemaCreated created:
                            Index(created)[created.SchemaId.Name] = created.SchemaId.Id;
                            break;
                        case SchemaDeleted deleted:
                            Index(deleted).Remove(deleted.SchemaId.Name);
                            break;
                    }
                }
            }

            foreach (var (appId, schemas) in schemasByApp)
            {
                await indexSchemas.RebuildAsync(appId, schemas);
            }
        }
    }
}
