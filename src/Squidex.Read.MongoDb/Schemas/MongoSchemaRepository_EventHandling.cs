// ==========================================================================
//  MongoSchemaRepository_EventHandling.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Core.Schemas;
using Squidex.Events.Schemas;
using Squidex.Events.Schemas.Utils;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;
using Squidex.Read.MongoDb.Utils;

namespace Squidex.Read.MongoDb.Schemas
{
    public partial class MongoSchemaRepository
    {
        public event Action<Guid> SchemaSaved;

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        protected async Task On(SchemaCreated @event, EnvelopeHeaders headers)
        {
            var schema = SchemaEventDispatcher.Dispatch(@event);

            await Collection.CreateAsync(headers, s => { UpdateSchema(s, schema); SimpleMapper.Map(@event, s); });

            SchemaSaved?.Invoke(headers.AggregateId());
        }

        protected Task On(FieldDeleted @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldDisabled @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldEnabled @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldHidden @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldShown @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(SchemaUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(SchemaPublished @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(SchemaUnpublished @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldAdded @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(headers, s => SchemaEventDispatcher.Dispatch(@event, s, registry));
        }

        protected async Task On(SchemaDeleted @event, EnvelopeHeaders headers)
        {
            await Collection.UpdateAsync(headers, s => s.IsDeleted = true);

            SchemaSaved?.Invoke(headers.AggregateId());
        }

        private async Task UpdateSchema(EnvelopeHeaders headers, Func<Schema, Schema> updater)
        {
            await Collection.UpdateAsync(headers, e => UpdateSchema(e, updater));

            SchemaSaved?.Invoke(headers.AggregateId());
        }

        private void UpdateSchema(MongoSchemaEntity entity, Func<Schema, Schema> updater)
        {
            entity.UpdateSchema(serializer, updater);
        }

        private void UpdateSchema(MongoSchemaEntity entity, Schema schema)
        {
            entity.SerializeSchema(schema, serializer);
        }
    }
}
