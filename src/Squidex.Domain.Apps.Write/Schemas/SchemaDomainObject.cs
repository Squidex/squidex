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
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Write.Schemas
{
    public class SchemaDomainObject : DomainObjectBase
    {
        private readonly FieldRegistry registry;
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

        protected void On(SchemaCreated @event)
        {
            totalFields += @event.Fields?.Count ?? 0;

            schema = SchemaEventDispatcher.Create(@event, registry);
        }

        public void On(FieldAdded @event)
        {
            totalFields++;

            schema = schema.Apply(@event, registry);
        }

        protected void On(FieldUpdated @event)
        {
            schema = schema.Apply(@event);
        }

        protected void On(FieldLocked @event)
        {
            schema = schema.Apply(@event);
        }

        protected void On(FieldHidden @event)
        {
            schema = schema.Apply(@event);
        }

        protected void On(FieldShown @event)
        {
            schema = schema.Apply(@event);
        }

        protected void On(FieldDisabled @event)
        {
            schema = schema.Apply(@event);
        }

        protected void On(FieldEnabled @event)
        {
            schema = schema.Apply(@event);
        }

        protected void On(SchemaUpdated @event)
        {
            schema = schema.Apply(@event);
        }

        protected void On(FieldDeleted @event)
        {
            schema = schema.Apply(@event);
        }

        protected void On(SchemaFieldsReordered @event)
        {
            schema = schema.Apply(@event);
        }

        protected void On(SchemaPublished @event)
        {
            schema = schema.Apply(@event);
        }

        protected void On(SchemaUnpublished @event)
        {
            schema = schema.Apply(@event);
        }

        protected void On(SchemaDeleted @event)
        {
            isDeleted = true;
        }

        public SchemaDomainObject Create(CreateSchema command)
        {
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

        public SchemaDomainObject Add(AddField command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new FieldAdded { FieldId = new NamedId<long>(totalFields + 1, command.Name) }));

            return this;
        }

        public SchemaDomainObject UpdateField(UpdateField command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(command, SimpleMapper.Map(command, new FieldUpdated()));

            return this;
        }

        public SchemaDomainObject LockField(LockField command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(command, new FieldLocked());

            return this;
        }

        public SchemaDomainObject HideField(HideField command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(command, new FieldHidden());

            return this;
        }

        public SchemaDomainObject ShowField(ShowField command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(command, new FieldShown());

            return this;
        }

        public SchemaDomainObject DisableField(DisableField command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(command, new FieldDisabled());

            return this;
        }

        public SchemaDomainObject EnableField(EnableField command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(command, new FieldEnabled());

            return this;
        }

        public SchemaDomainObject DeleteField(DeleteField command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(command, new FieldDeleted());

            return this;
        }

        public SchemaDomainObject Reorder(ReorderFields command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new SchemaFieldsReordered()));

            return this;
        }

        public SchemaDomainObject Publish(PublishSchema command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new SchemaPublished()));

            return this;
        }

        public SchemaDomainObject Unpublish(UnpublishSchema command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new SchemaUnpublished()));

            return this;
        }

        public SchemaDomainObject ConfigureScripts(ConfigureScripts command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new ScriptsConfigured()));

            return this;
        }

        public SchemaDomainObject Delete(DeleteSchema command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new SchemaDeleted()));

            return this;
        }

        public SchemaDomainObject Update(UpdateSchema command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new SchemaUpdated()));

            return this;
        }

        protected void RaiseEvent(FieldCommand fieldCommand, FieldEvent @event)
        {
            SimpleMapper.Map(fieldCommand, @event);

            if (schema.FieldsById.TryGetValue(fieldCommand.FieldId, out var field))
            {
                @event.FieldId = new NamedId<long>(field.Id, field.Name);
            }

            RaiseEvent(@event);
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