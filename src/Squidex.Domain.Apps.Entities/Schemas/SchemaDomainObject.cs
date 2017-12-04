// ==========================================================================
//  SchemaDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public class SchemaDomainObject : DomainObjectBase<SchemaDomainObject, SchemaState>
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

            var @event = SimpleMapper.Map(command, new SchemaCreated { SchemaId = new NamedId<Guid>(State.Id, command.Name) });

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

            var partitioning =
                string.Equals(command.Partitioning, Partitioning.Language.Key, StringComparison.OrdinalIgnoreCase) ?
                    Partitioning.Language :
                    Partitioning.Invariant;

            var fieldId = State.TotalFields;

            var field = registry.CreateField(fieldId, command.Name, partitioning, command.Properties);

            UpdateState(command, state =>
            {
                state.SchemaDef = state.SchemaDef.AddField(field);
                state.TotalFields = fieldId + 1;
            });

            RaiseEvent(SimpleMapper.Map(command, new FieldAdded { FieldId = new NamedId<long>(fieldId + 1, command.Name) }));

            return this;
        }

        public SchemaDomainObject UpdateField(UpdateField command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateSchema(command, s => s.UpdateField(command.FieldId, command.Properties));

            RaiseEvent(command, SimpleMapper.Map(command, new FieldUpdated()));

            return this;
        }

        public SchemaDomainObject LockField(LockField command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateSchema(command, s => s.LockField(command.FieldId));

            RaiseEvent(command, new FieldLocked());

            return this;
        }

        public SchemaDomainObject HideField(HideField command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateSchema(command, s => s.HideField(command.FieldId));

            RaiseEvent(command, new FieldHidden());

            return this;
        }

        public SchemaDomainObject ShowField(ShowField command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateSchema(command, s => s.ShowField(command.FieldId));

            RaiseEvent(command, new FieldShown());

            return this;
        }

        public SchemaDomainObject DisableField(DisableField command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateSchema(command, s => s.DisableField(command.FieldId));

            RaiseEvent(command, new FieldDisabled());

            return this;
        }

        public SchemaDomainObject EnableField(EnableField command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateSchema(command, s => s.EnableField(command.FieldId));

            RaiseEvent(command, new FieldEnabled());

            return this;
        }

        public SchemaDomainObject DeleteField(DeleteField command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateSchema(command, s => s.DeleteField(command.FieldId));

            RaiseEvent(command, new FieldDeleted());

            return this;
        }

        public SchemaDomainObject Reorder(ReorderFields command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateSchema(command, s => s.ReorderFields(command.FieldIds));

            RaiseEvent(SimpleMapper.Map(command, new SchemaFieldsReordered()));

            return this;
        }

        public SchemaDomainObject Publish(PublishSchema command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateSchema(command, s => s.Publish());

            RaiseEvent(SimpleMapper.Map(command, new SchemaPublished()));

            return this;
        }

        public SchemaDomainObject Unpublish(UnpublishSchema command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateSchema(command, s => s.Unpublish());

            RaiseEvent(SimpleMapper.Map(command, new SchemaUnpublished()));

            return this;
        }

        public SchemaDomainObject ConfigureScripts(ConfigureScripts command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateState(command, s => SimpleMapper.Map(command, s));

            RaiseEvent(SimpleMapper.Map(command, new ScriptsConfigured()));

            return this;
        }

        public SchemaDomainObject Delete(DeleteSchema command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateState(command, s => s.IsDeleted = true);

            RaiseEvent(SimpleMapper.Map(command, new SchemaDeleted()));

            return this;
        }

        public SchemaDomainObject Update(UpdateSchema command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateState(command, s => SimpleMapper.Map(command, s));

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

        private void UpdateSchema(ICommand command, Func<Schema, Schema> updater)
        {
            UpdateState(command, s => s.SchemaDef = updater(s.SchemaDef));
        }

        protected override SchemaState CloneState(ICommand command, Action<SchemaState> updater)
        {
            return State.Clone().Update((SquidexCommand)command, updater);
        }
    }
}