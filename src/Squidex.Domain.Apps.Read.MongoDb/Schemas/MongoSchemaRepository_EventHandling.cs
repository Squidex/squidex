// ==========================================================================
//  MongoSchemaRepository_EventHandling.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Domain.Apps.Events.Schemas.Old;
using Squidex.Domain.Apps.Events.Schemas.Utils;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

#pragma warning disable CS0612 // Type or member is obsolete

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
            return Collection.CreateAsync(@event, headers, s =>
            {
                s.SchemaDef = SchemaEventDispatcher.Create(@event, registry);

                SimpleMapper.Map(@event, s);
            });
        }

        protected Task On(FieldDeleted @event, EnvelopeHeaders headers)
        {
            return UpdateSchemaAsync(@event, headers, s =>
            {
                s.SchemaDef.Apply(@event);
            });
        }

        protected Task On(FieldLocked @event, EnvelopeHeaders headers)
        {
            return UpdateSchemaAsync(@event, headers, s =>
            {
                s.SchemaDef.Apply(@event);
            });
        }

        protected Task On(FieldHidden @event, EnvelopeHeaders headers)
        {
            return UpdateSchemaAsync(@event, headers, s =>
            {
                s.SchemaDef.Apply(@event);
            });
        }

        protected Task On(FieldShown @event, EnvelopeHeaders headers)
        {
            return UpdateSchemaAsync(@event, headers, s =>
            {
                s.SchemaDef.Apply(@event);
            });
        }

        protected Task On(FieldDisabled @event, EnvelopeHeaders headers)
        {
            return UpdateSchemaAsync(@event, headers, s =>
            {
                s.SchemaDef.Apply(@event);
            });
        }

        protected Task On(FieldEnabled @event, EnvelopeHeaders headers)
        {
            return UpdateSchemaAsync(@event, headers, s =>
            {
                s.SchemaDef.Apply(@event);
            });
        }

        protected Task On(FieldUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateSchemaAsync(@event, headers, s =>
            {
                s.SchemaDef.Apply(@event);
            });
        }

        protected Task On(SchemaFieldsReordered @event, EnvelopeHeaders headers)
        {
            return UpdateSchemaAsync(@event, headers, s =>
            {
                s.SchemaDef.Apply(@event);
            });
        }

        protected Task On(SchemaUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateSchemaAsync(@event, headers, s =>
            {
                s.SchemaDef.Apply(@event);
            });
        }

        protected Task On(SchemaPublished @event, EnvelopeHeaders headers)
        {
            return UpdateSchemaAsync(@event, headers, s =>
            {
                s.SchemaDef.Apply(@event);
            });
        }

        protected Task On(SchemaUnpublished @event, EnvelopeHeaders headers)
        {
            return UpdateSchemaAsync(@event, headers, s =>
            {
                s.SchemaDef.Apply(@event);
            });
        }

        protected Task On(FieldAdded @event, EnvelopeHeaders headers)
        {
            return UpdateSchemaAsync(@event, headers, s =>
            {
                s.SchemaDef.Apply(@event, registry);
            });
        }

        protected Task On(ScriptsConfigured @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, s =>
            {
                SimpleMapper.Map(@event, s);
            });
        }

        protected Task On(SchemaDeleted @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, s =>
            {
                s.IsDeleted = true;
            });
        }

        protected Task On(WebhookAdded @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, s =>
            {
                /* NOOP */
            });
        }

        protected Task On(WebhookDeleted @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, s =>
            {
                /* NOOP */
            });
        }

        private Task UpdateSchemaAsync(SquidexEvent @event, EnvelopeHeaders headers, Action<MongoSchemaEntity> updater)
        {
            return Collection.UpdateAsync(@event, headers, s =>
            {
                updater(s);

                s.IsPublished = s.SchemaDef.IsPublished;
            });
        }
    }
}
