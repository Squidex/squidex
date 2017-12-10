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
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public class SchemaDomainObject : DomainObjectBase<SchemaState>
    {
        private readonly FieldRegistry registry;

        public SchemaDomainObject(FieldRegistry registry)
        {
            Guard.NotNull(registry, nameof(registry));

            this.registry = registry;
        }

        public SchemaDomainObject Create(CreateSchema command)
        {
            VerifyNotCreated();

            var @event = SimpleMapper.Map(command, new SchemaCreated { SchemaId = new NamedId<Guid>(command.SchemaId, command.Name) });

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

            RaiseEvent(SimpleMapper.Map(command, new FieldAdded { FieldId = new NamedId<long>(State.TotalFields + 1, command.Name) }));

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

            if (State.SchemaDef.FieldsById.TryGetValue(fieldCommand.FieldId, out var field))
            {
                @event.FieldId = new NamedId<long>(field.Id, field.Name);
            }

            RaiseEvent(@event);
        }

        private void VerifyNotCreated()
        {
            if (State.SchemaDef != null)
            {
                throw new DomainException("Schema has already been created.");
            }
        }

        private void VerifyCreatedAndNotDeleted()
        {
            if (State.IsDeleted || State.SchemaDef == null)
            {
                throw new DomainException("Schema has already been deleted or not created yet.");
            }
        }

        protected override void OnRaised(Envelope<IEvent> @event)
        {
            UpdateState(State.Apply(@event, registry));
        }
    }
}