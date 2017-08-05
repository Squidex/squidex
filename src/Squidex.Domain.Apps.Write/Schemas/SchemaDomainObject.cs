// ==========================================================================
//  SchemaDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Domain.Apps.Events.Schemas.Utils;
using Squidex.Domain.Apps.Write.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Write.Schemas
{
    public class SchemaDomainObject : DomainObjectBase
    {
        private readonly FieldRegistry registry;
        private readonly HashSet<Guid> webhookIds = new HashSet<Guid>();
        private bool isDeleted;
        private long totalFields;
        private Schema schema;

        public Schema Schema
        {
            get { return schema; }
        }

        public bool IsDeleted
        {
            get { return isDeleted; }
        }

        public SchemaDomainObject(Guid id, int version, FieldRegistry registry)
            : base(id, version)
        {
            Guard.NotNull(registry, nameof(registry));

            this.registry = registry;
        }

        public void On(FieldAdded @event)
        {
            totalFields++;

            schema = SchemaEventDispatcher.Dispatch(@event, schema, registry);
        }

        protected void On(SchemaCreated @event)
        {
            totalFields += @event.Fields?.Count ?? 0;

            schema = SchemaEventDispatcher.Dispatch(@event, registry);
        }

        protected void On(FieldUpdated @event)
        {
            schema = SchemaEventDispatcher.Dispatch(@event, schema);
        }

        protected void On(FieldHidden @event)
        {
            schema = SchemaEventDispatcher.Dispatch(@event, schema);
        }

        protected void On(FieldShown @event)
        {
            schema = SchemaEventDispatcher.Dispatch(@event, schema);
        }

        protected void On(FieldDisabled @event)
        {
            schema = SchemaEventDispatcher.Dispatch(@event, schema);
        }

        protected void On(FieldEnabled @event)
        {
            schema = SchemaEventDispatcher.Dispatch(@event, schema);
        }

        protected void On(SchemaUpdated @event)
        {
            schema = SchemaEventDispatcher.Dispatch(@event, schema);
        }

        protected void On(FieldDeleted @event)
        {
            schema = SchemaEventDispatcher.Dispatch(@event, schema);
        }

        protected void On(SchemaFieldsReordered @event)
        {
            schema = SchemaEventDispatcher.Dispatch(@event, schema);
        }

        protected void On(SchemaPublished @event)
        {
            schema = SchemaEventDispatcher.Dispatch(@event, schema);
        }

        protected void On(SchemaUnpublished @event)
        {
            schema = SchemaEventDispatcher.Dispatch(@event, schema);
        }

        protected void On(WebhookAdded @event)
        {
            webhookIds.Add(@event.Id);
        }

        protected void On(WebhookDeleted @event)
        {
            webhookIds.Remove(@event.Id);
        }

        protected void On(SchemaDeleted @event)
        {
            isDeleted = true;
        }

        public SchemaDomainObject Create(CreateSchema command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot create schema");

            VerifyNotCreated();

            var @event = SimpleMapper.Map(command, new SchemaCreated { SchemaId = new NamedId<Guid>(Id, command.Name) });

            if (command.Fields != null)
            {
                @event.Fields = new List<SchemaCreatedField>();

                foreach (var commandField in command.Fields)
                {
                    var eventField = SimpleMapper.Map(commandField, new SchemaCreatedField());

                    @event.Fields.Add(eventField);
                }
            }

            RaiseEvent(@event);

            return this;
        }

        public SchemaDomainObject DeleteWebhook(DeleteWebhook command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();
            VerifyWebhookExists(command.Id);

            RaiseEvent(SimpleMapper.Map(command, new WebhookDeleted()));

            return this;
        }

        public SchemaDomainObject AddWebhook(AddWebhook command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot add webhook");

            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new WebhookAdded()));

            return this;
        }

        public SchemaDomainObject AddField(AddField command)
        {
            Guard.Valid(command, nameof(command), () => $"Cannot add field to schema {Id}");

            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new FieldAdded { FieldId = new NamedId<long>(totalFields + 1, command.Name) }));

            return this;
        }

        public SchemaDomainObject UpdateField(UpdateField command)
        {
            Guard.Valid(command, nameof(command), () => $"Cannot update schema '{Id}'");

            VerifyCreatedAndNotDeleted();

            RaiseEvent(command, SimpleMapper.Map(command, new FieldUpdated()));

            return this;
        }

        public SchemaDomainObject Reorder(ReorderFields command)
        {
            Guard.Valid(command, nameof(command), () => $"Cannot reorder fields for schema '{Id}'");

            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new SchemaFieldsReordered()));

            return this;
        }

        public SchemaDomainObject Update(UpdateSchema command)
        {
            Guard.Valid(command, nameof(command), () => $"Cannot update schema '{Id}'");

            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new SchemaUpdated()));

            return this;
        }

        public SchemaDomainObject HideField(HideField command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();

            RaiseEvent(command, new FieldHidden());

            return this;
        }

        public SchemaDomainObject ShowField(ShowField command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();

            RaiseEvent(command, new FieldShown());

            return this;
        }

        public SchemaDomainObject DisableField(DisableField command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();

            RaiseEvent(command, new FieldDisabled());

            return this;
        }

        public SchemaDomainObject EnableField(EnableField command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();

            RaiseEvent(command, new FieldEnabled());

            return this;
        }

        public SchemaDomainObject DeleteField(DeleteField command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();

            RaiseEvent(command, new FieldDeleted());

            return this;
        }

        public SchemaDomainObject Publish(PublishSchema command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new SchemaPublished()));

            return this;
        }

        public SchemaDomainObject Unpublish(UnpublishSchema command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new SchemaUnpublished()));

            return this;
        }

        public SchemaDomainObject Delete(DeleteSchema command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new SchemaDeleted()));

            return this;
        }

        protected void RaiseEvent(FieldCommand fieldCommand, FieldEvent @event)
        {
            SimpleMapper.Map(fieldCommand, @event);

            if (schema.FieldsById.TryGetValue(fieldCommand.FieldId, out Field field))
            {
                @event.FieldId = new NamedId<long>(field.Id, field.Name);
            }
            else
            {
                throw new DomainObjectNotFoundException(fieldCommand.FieldId.ToString(), "Fields", typeof(Field));
            }

            RaiseEvent(@event);
        }

        private void VerifyWebhookExists(Guid id)
        {
            if (!webhookIds.Contains(id))
            {
                throw new DomainObjectNotFoundException(id.ToString(), "Webhooks", typeof(Schema));
            }
        }

        private void VerifyNotCreated()
        {
            if (schema != null)
            {
                throw new DomainException("Schema has already been created.");
            }
        }

        private void VerifyCreatedAndNotDeleted()
        {
            if (isDeleted || schema == null)
            {
                throw new DomainException("Schema has already been deleted or not created yet.");
            }
        }

        protected override void DispatchEvent(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload);
        }
    }
}