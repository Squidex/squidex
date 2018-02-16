// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.State;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Migrate_01
{
    public sealed class Rebuilder
    {
        private readonly FieldRegistry fieldRegistry;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly ISnapshotStore<AppState, Guid> snapshotAppStore;
        private readonly ISnapshotStore<AssetState, Guid> snapshotAssetStore;
        private readonly ISnapshotStore<ContentState, Guid> snapshotContentStore;
        private readonly ISnapshotStore<RuleState, Guid> snapshotRuleStore;
        private readonly ISnapshotStore<SchemaState, Guid> snapshotSchemaStore;
        private readonly IStateFactory stateFactory;

        public Rebuilder(
            FieldRegistry fieldRegistry,
            IEventDataFormatter eventDataFormatter,
            IEventStore eventStore,
            ISnapshotStore<AppState, Guid> snapshotAppStore,
            ISnapshotStore<ContentState, Guid> snapshotContentStore,
            ISnapshotStore<AssetState, Guid> snapshotAssetStore,
            ISnapshotStore<RuleState, Guid> snapshotRuleStore,
            ISnapshotStore<SchemaState, Guid> snapshotSchemaStore,
            IStateFactory stateFactory)
        {
            this.fieldRegistry = fieldRegistry;
            this.eventDataFormatter = eventDataFormatter;
            this.eventStore = eventStore;
            this.snapshotAppStore = snapshotAppStore;
            this.snapshotAssetStore = snapshotAssetStore;
            this.snapshotContentStore = snapshotContentStore;
            this.snapshotRuleStore = snapshotRuleStore;
            this.snapshotSchemaStore = snapshotSchemaStore;
            this.stateFactory = stateFactory;
        }

        public async Task RebuildAssetsAsync()
        {
            await snapshotAssetStore.ClearAsync();

            const string filter = "^asset\\-";

            var handledIds = new HashSet<Guid>();

            await eventStore.QueryAsync(async storedEvent =>
            {
                var @event = ParseKnownEvent(storedEvent);

                if (@event != null)
                {
                    if (@event.Payload is AssetEvent assetEvent && handledIds.Add(assetEvent.AssetId))
                    {
                        var asset = await stateFactory.CreateAsync<AssetGrain>(assetEvent.AssetId);

                        asset.ApplySnapshot(asset.Snapshot.Apply(@event));

                        await asset.WriteSnapshotAsync();
                    }
                }
            }, filter, ct: CancellationToken.None);
        }

        public async Task RebuildConfigAsync()
        {
            await snapshotAppStore.ClearAsync();
            await snapshotRuleStore.ClearAsync();
            await snapshotSchemaStore.ClearAsync();

            const string filter = "^((app\\-)|(schema\\-)|(rule\\-))";

            var handledIds = new HashSet<Guid>();

            await eventStore.QueryAsync(async storedEvent =>
            {
                var @event = ParseKnownEvent(storedEvent);

                if (@event != null)
                {
                    if (@event.Payload is SchemaEvent schemaEvent && handledIds.Add(schemaEvent.SchemaId.Id))
                    {
                        var schema = await stateFactory.GetSingleAsync<SchemaGrain>(schemaEvent.SchemaId.Id);

                        await schema.WriteSnapshotAsync();
                    }
                    else if (@event.Payload is RuleEvent ruleEvent && handledIds.Add(ruleEvent.RuleId))
                    {
                        var rule = await stateFactory.GetSingleAsync<RuleGrain>(ruleEvent.RuleId);

                        await rule.WriteSnapshotAsync();
                    }
                    else if (@event.Payload is AppEvent appEvent && handledIds.Add(appEvent.AppId.Id))
                    {
                        var app = await stateFactory.GetSingleAsync<AppGrain>(appEvent.AppId.Id);

                        await app.WriteSnapshotAsync();
                    }
                }
            }, filter, ct: CancellationToken.None);
        }

        public async Task RebuildContentAsync()
        {
            await snapshotContentStore.ClearAsync();

            const string filter = "^((content\\-))";

            var handledIds = new HashSet<Guid>();

            await eventStore.QueryAsync(async storedEvent =>
            {
                var @event = ParseKnownEvent(storedEvent);

                if (@event.Payload is ContentEvent contentEvent)
                {
                    try
                    {
                        var (content, version) = await snapshotContentStore.ReadAsync(contentEvent.ContentId);

                        if (content == null)
                        {
                            version = EtagVersion.Empty;

                            content = new ContentState();
                        }

                        content = content.Apply(@event);

                        await snapshotContentStore.WriteAsync(contentEvent.ContentId, content, version, version + 1);
                    }
                    catch (DomainObjectNotFoundException)
                    {
                        // Schema has been deleted.
                    }
                }
            }, filter, ct: CancellationToken.None);
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
