// ==========================================================================
//  ModelSchemaDomainObject.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Core.Schema;
using PinkParrot.Events.Schema;
using PinkParrot.Infrastructure.CQRS;
using PinkParrot.Infrastructure.CQRS.Events;
using PinkParrot.Infrastructure.Dispatching;
using PinkParrot.Write.Schema.Commands;

namespace PinkParrot.Write.Schema
{
    public class ModelSchemaDomainObject : DomainObject
    {
        private readonly ModelFieldFactory fieldFactory;
        private bool isDeleted;
        private long totalFields;
        private ModelSchema schema;

        public ModelSchema Schema
        {
            get { return schema; }
        }

        public bool IsDeleted
        {
            get { return isDeleted; }
        }

        public ModelSchemaDomainObject(Guid id, int version, ModelFieldFactory fieldFactory)
            : base(id, version)
        {
            this.fieldFactory = fieldFactory;
        }

        protected void Apply(ModelFieldAdded @event)
        {
            schema = schema.AddField(@event.FieldId, @event.FieldType, @event.FieldName, fieldFactory);

            totalFields++;
        }

        protected void Apply(ModelSchemaCreated @event)
        {
            schema = ModelSchema.Create(@event.Name);
        }

        protected void Apply(ModelFieldUpdated @event)
        {
            schema = schema.SetField(@event.FieldId, @event.Settings);
        }

        public void Apply(ModelFieldHidden @event)
        {
            schema = schema.HideField(@event.FieldId);
        }

        public void Apply(ModelFieldShown @event)
        {
            schema = schema.ShowField(@event.FieldId);
        }

        public void Apply(ModelFieldDisabled @event)
        {
            schema = schema.DisableField(@event.FieldId);
        }

        public void Apply(ModelFieldEnabled @event)
        {
            schema = schema.EnableField(@event.FieldId);
        }

        protected void Apply(ModelSchemaUpdated @event)
        {
            schema = schema.Update(schema.Metadata.Configure(@event.NewName, @event.Settings));
        }

        protected void Apply(ModelFieldDeleted @event)
        {
            schema = schema.DeleteField(@event.FieldId);
        }

        protected void Apply(ModelSchemaDeleted @event)
        {
            isDeleted = false;
        }

        public void AddField(AddModelField command)
        {
            VerifyCreatedAndNotDeleted();

            var id = ++totalFields;

            schema = schema.AddField(id, command.FieldType, command.FieldName, fieldFactory);

            RaiseEvent(new ModelFieldAdded { FieldId = id, FieldType = command.FieldType, FieldName = command.FieldName }, true);
        }

        public void Create(CreateModelSchema command)
        {
            VerifyNotCreated();

            schema = ModelSchema.Create(command.Name);

            RaiseEvent(new ModelSchemaCreated { Name = command.Name }, true);
        }

        public void Update(UpdateModelSchema command)
        {
            VerifyCreatedAndNotDeleted();

            schema = schema.Update(schema.Metadata.Configure(command.NewName, command.Settings));

            RaiseEvent(new ModelSchemaUpdated { NewName = command.NewName, Settings = command.Settings }, true);
        }

        public void UpdateField(UpdateModelField command)
        {
            VerifyCreatedAndNotDeleted();

            schema = schema.SetField(command.FieldId, command.Settings);

            RaiseEvent(new ModelFieldUpdated { FieldId = command.FieldId, Settings = command.Settings }, true);
        }

        public void HideField(HideModelField command)
        {
            VerifyCreatedAndNotDeleted();

            schema = schema.HideField(command.FieldId);

            RaiseEvent(new ModelFieldHidden { FieldId = command.FieldId }, true);
        }

        public void ShowField(ShowModelField command)
        {
            VerifyCreatedAndNotDeleted();

            schema = schema.ShowField(command.FieldId);

            RaiseEvent(new ModelFieldShown { FieldId = command.FieldId }, true);
        }

        public void DisableField(DisableModelField command)
        {
            VerifyCreatedAndNotDeleted();

            schema = schema.DisableField(command.FieldId);

            RaiseEvent(new ModelFieldDisabled { FieldId = command.FieldId }, true);
        }

        public void EnableField(EnableModelField command)
        {
            VerifyCreatedAndNotDeleted();

            schema = schema.EnableField(command.FieldId);

            RaiseEvent(new ModelFieldEnabled { FieldId = command.FieldId }, true);
        }

        public void Delete(DeleteModelSchema command)
        {
            VerifyCreatedAndNotDeleted();

            isDeleted = true;

            RaiseEvent(new ModelSchemaDeleted(), true);
        }

        public void DeleteField(DeleteModelField command)
        {
            VerifyCreatedAndNotDeleted();

            schema = schema.DeleteField(command.FieldId);

            RaiseEvent(new ModelFieldDeleted { FieldId = command.FieldId }, true);
        }

        private void VerifyNotCreated()
        {
            if (schema != null)
            {
                throw new InvalidOperationException("Schema has already been created.");
            }
        }

        private void VerifyCreatedAndNotDeleted()
        {
            if (isDeleted || schema == null)
            {
                throw new InvalidOperationException("Schema has already been deleted or not created yet.");
            }
        }

        protected override void ApplyEvent(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload);
        }
    }
}