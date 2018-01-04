// ==========================================================================
//  MigrateToEntities.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;

namespace Migrate_01
{
    public sealed class Migration01_FromCqrs : IMigration
    {
        private readonly FieldRegistry fieldRegistry;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly ISnapshotStore<ContentState, Guid> snapshotStore;
        private readonly IStateFactory stateFactory;

        public int FromVersion { get; } = 0;

        public int ToVersion { get; } = 1;

        public Migration01_FromCqrs(
            FieldRegistry fieldRegistry,
            IEventDataFormatter eventDataFormatter,
            IEventStore eventStore,
            ISnapshotStore<ContentState, Guid> snapshotStore,
            IStateFactory stateFactory)
        {
            this.fieldRegistry = fieldRegistry;
            this.eventDataFormatter = eventDataFormatter;
            this.eventStore = eventStore;
            this.snapshotStore = snapshotStore;
            this.stateFactory = stateFactory;
        }

        public async Task UpdateAsync()
        {
            await MigrateConfigAsync();
            await MigrateContentAsync();
            await MigrateAssetsAsync();
        }

        private Task MigrateAssetsAsync()
        {
            const string filter = "^asset\\-";

            var handledIds = new HashSet<Guid>();

            return eventStore.GetEventsAsync(async storedEvent =>
            {
                var @event = ParseKnownEvent(storedEvent);

                if (@event != null)
                {
                    if (@event.Payload is AssetEvent assetEvent && handledIds.Add(assetEvent.AssetId))
                    {
                        var asset = await stateFactory.CreateAsync<AssetDomainObject>(assetEvent.AssetId);

                        asset.ApplySnapshot(asset.Snapshot.Apply(@event));

                        await asset.WriteSnapshotAsync();
                    }
                }
            }, CancellationToken.None, filter);
        }

        private Task MigrateConfigAsync()
        {
            const string filter = "^((app\\-)|(schema\\-)|(rule\\-))";

            var handledIds = new HashSet<Guid>();

            return eventStore.GetEventsAsync(async storedEvent =>
            {
                var @event = ParseKnownEvent(storedEvent);

                if (@event != null)
                {
                    if (@event.Payload is SchemaEvent schemaEvent && handledIds.Add(schemaEvent.SchemaId.Id))
                    {
                        var schema = await stateFactory.GetSingleAsync<SchemaDomainObject>(schemaEvent.SchemaId.Id);

                        await schema.WriteSnapshotAsync();
                    }
                    else if (@event.Payload is RuleEvent ruleEvent && handledIds.Add(ruleEvent.RuleId))
                    {
                        var rule = await stateFactory.GetSingleAsync<RuleDomainObject>(ruleEvent.RuleId);

                        await rule.WriteSnapshotAsync();
                    }
                    else if (@event.Payload is AppEvent appEvent && handledIds.Add(appEvent.AppId.Id))
                    {
                        var app = await stateFactory.GetSingleAsync<AppDomainObject>(appEvent.AppId.Id);

                        await app.WriteSnapshotAsync();
                    }
                }
            }, CancellationToken.None, filter);
        }

        private Task MigrateContentAsync()
        {
            const string filter = "^((content\\-))";

            var handledIds = new HashSet<Guid>();

            return eventStore.GetEventsAsync(async storedEvent =>
            {
                var @event = ParseKnownEvent(storedEvent);

                if (@event.Payload is ContentEvent contentEvent)
                {
                    try
                    {
                        var (content, version) = await snapshotStore.ReadAsync(contentEvent.ContentId);

                        if (content == null)
                        {
                            version = EtagVersion.Empty;

                            content = new ContentState();
                        }

                        content = content.Apply(@event);

                        await snapshotStore.WriteAsync(contentEvent.ContentId, content, version, version + 1);
                    }
                    catch (DomainObjectNotFoundException)
                    {
                        // Schema has been deleted.
                    }
                }
            }, CancellationToken.None, filter);
        }

        private Envelope<IEvent> ParseKnownEvent(StoredEvent storedEvent)
        {
            try
            {
                return eventDataFormatter.Parse(storedEvent.Data);
            }
            catch (TypeNameNotFoundException)
            {
                return null;
            }
        }
    }
}
