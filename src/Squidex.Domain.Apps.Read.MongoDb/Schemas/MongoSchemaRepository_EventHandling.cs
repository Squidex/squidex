// ==========================================================================
//  MongoSchemaRepository_EventHandling.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Domain.Apps.Events.Schemas.Utils;
using Squidex.Domain.Apps.Read.MongoDb.Utils;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Read.MongoDb.Schemas
{
    public partial class MongoSchemaRepository
    {
        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^schema-"; }
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        protected Task On(SchemaCreated @event, EnvelopeHeaders headers)
        {
            var schema = SchemaEventDispatcher.Dispatch(@event, registry);

            return Collection.CreateAsync(@event, headers, s => { UpdateSchema(s, schema); SimpleMapper.Map(@event, s); });
        }

        protected Task On(FieldDeleted @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(@event, headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldLocked @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(@event, headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldHidden @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(@event, headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldShown @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(@event, headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldDisabled @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(@event, headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldEnabled @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(@event, headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(@event, headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(SchemaFieldsReordered @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(@event, headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(SchemaUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(@event, headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(SchemaPublished @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(@event, headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(SchemaUnpublished @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(@event, headers, s => SchemaEventDispatcher.Dispatch(@event, s));
        }

        protected Task On(FieldAdded @event, EnvelopeHeaders headers)
        {
            return UpdateSchema(@event, headers, s => SchemaEventDispatcher.Dispatch(@event, s, registry));
        }

        protected Task On(SchemaDeleted @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, e => e.IsDeleted = true);
        }

        private Task UpdateSchema(SquidexEvent @event, EnvelopeHeaders headers, Func<Schema, Schema> updater)
        {
            return Collection.UpdateAsync(@event, headers, e => UpdateSchema(e, updater));
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
