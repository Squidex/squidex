// ==========================================================================
//  MigrateToEntities.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
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
        private readonly IStateFactory stateFactory;

        public int FromVersion { get; } = 0;

        public int ToVersion { get; } = 1;

        public Migration01_FromCqrs(
            FieldRegistry fieldRegistry,
            IEventDataFormatter eventDataFormatter,
            IEventStore eventStore,
            IStateFactory stateFactory)
        {
            this.fieldRegistry = fieldRegistry;
            this.eventDataFormatter = eventDataFormatter;
            this.eventStore = eventStore;
            this.stateFactory = stateFactory;
        }

        public async Task UpdateAsync()
        {
            await eventStore.GetEventsAsync(async storedEvent =>
            {
                var @event = ParseKnownEvent(storedEvent);

                if (@event != null)
                {
                    var version = storedEvent.EventStreamNumber;

                    if (@event.Payload is AssetEvent assetEvent)
                    {
                        var asset = await stateFactory.CreateAsync<AssetDomainObject>(assetEvent.AssetId);

                        asset.UpdateState(asset.State.Apply(@event));

                        await asset.WriteStateAsync(version);
                    }
                    else if (@event.Payload is ContentEvent contentEvent)
                    {
                        var content = await stateFactory.CreateAsync<ContentDomainObject>(contentEvent.ContentId);

                        content.UpdateState(content.State.Apply(@event));

                        await content.WriteStateAsync(version);
                    }
                    else if (@event.Payload is SchemaEvent schemaEvent)
                    {
                        var schema = await stateFactory.GetSingleAsync<SchemaDomainObject>(schemaEvent.SchemaId.Id);

                        schema.UpdateState(schema.State.Apply(@event, fieldRegistry));

                        await schema.WriteStateAsync(version);
                    }
                    else if (@event.Payload is AppEvent appEvent)
                    {
                        var app = await stateFactory.GetSingleAsync<AppDomainObject>(appEvent.AppId.Id);

                        app.UpdateState(app.State.Apply(@event));

                        await app.WriteStateAsync(version);
                    }
                }
            }, CancellationToken.None);
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
