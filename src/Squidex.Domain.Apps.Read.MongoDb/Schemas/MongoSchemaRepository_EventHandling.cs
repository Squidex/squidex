// ==========================================================================
//  MongoSchemaRepository_EventHandling.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Domain.Apps.Events.Schemas.Old;
using Squidex.Domain.Apps.Events.Schemas.Utils;
using Squidex.Domain.Apps.Read.MongoDb.Utils;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

#pragma warning disable CS0612 // Type or member is obsolete

namespace Squidex.Domain.Apps.Read.MongoDb.Schemas
{
    public partial class MongoSchemaRepository
    {
        private readonly List<Action<NamedId<Guid>, NamedId<Guid>>> subscribers = new List<Action<NamedId<Guid>, NamedId<Guid>>>();

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^schema-"; }
        }

        public void SubscribeOnChanged(Action<NamedId<Guid>, NamedId<Guid>> subscriber)
        {
            subscribers.Add(subscriber);
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        protected Task On(SchemaCreated @event, EnvelopeHeaders headers)
        {
            var schema = SchemaEventDispatcher.Dispatch(@event, registry);

            return Collection.CreateAsync(@event, headers, s => { s.SchemaDef = schema; SimpleMapper.Map(@event, s); });
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

        protected Task On(ScriptsConfigured @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, e => SimpleMapper.Map(@event, e));
        }

        protected Task On(SchemaDeleted @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, e => e.IsDeleted = true);
        }

        protected Task On(WebhookAdded @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, e => { });
        }

        protected Task On(WebhookDeleted @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, e => { });
        }

        private async Task UpdateSchema(SchemaEvent @event, EnvelopeHeaders headers, Func<Schema, Schema> updater = null)
        {
            await Collection.UpdateAsync(@event, headers, e =>
            {
                if (updater != null)
                {
                    e.SchemaDef = updater(e.SchemaDef);
                }
            });

            foreach (var subscriber in subscribers)
            {
                subscriber(@event.AppId, @event.SchemaId);
            }
        }
    }
}
