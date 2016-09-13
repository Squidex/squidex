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
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS;
using PinkParrot.Infrastructure.CQRS.Events;
using PinkParrot.Infrastructure.Dispatching;

namespace PinkParrot.Write.Schema
{
    public class ModelSchemaDomainObject : DomainObject, ITenantAggregate
    {
        private readonly ModelFieldRegistry registry;
        private Guid tenantId;
        private bool isDeleted;
        private long totalFields;
        private ModelSchema schema;

        public ModelSchema Schema
        {
            get { return schema; }
        }

        public Guid TenantId
        {
            get { return tenantId; }
        }

        public bool IsDeleted
        {
            get { return isDeleted; }
        }

        public ModelSchemaDomainObject(Guid id, int version, ModelFieldRegistry registry)
            : base(id, version)
        {
            Guard.NotNull(registry, nameof(registry));

            this.registry = registry;
        }

        public void On(ModelFieldAdded @event)
        {
            schema = schema.AddOrUpdateField(registry.CreateField(@event.FieldId, @event.Name, @event.Properties));

            totalFields++;
        }

        public void On(ModelSchemaCreated @event)
        {
            tenantId = @event.TenantId;

            schema = ModelSchema.Create(@event.Name, @event.Properties);
        }

        public void On(ModelFieldUpdated @event)
        {
            schema = schema.UpdateField(@event.FieldId, @event.Properties);
        }

        public void On(ModelFieldHidden @event)
        {
            schema = schema.HideField(@event.FieldId);
        }

        public void On(ModelFieldShown @event)
        {
            schema = schema.ShowField(@event.FieldId);
        }

        public void On(ModelFieldDisabled @event)
        {
            schema = schema.DisableField(@event.FieldId);
        }

        public void On(ModelFieldEnabled @event)
        {
            schema = schema.EnableField(@event.FieldId);
        }

        public void On(ModelSchemaUpdated @event)
        {
            schema = schema.Update(@event.Properties);
        }

        public void On(ModelFieldDeleted @event)
        {
            schema = schema.DeleteField(@event.FieldId);
        }

        public void On(ModelSchemaDeleted @event)
        {
            isDeleted = false;
        }

        public void AddField(string name, IModelFieldProperties properties)
        {
            VerifyCreatedAndNotDeleted();
            
            RaiseEvent(new ModelFieldAdded { FieldId = ++totalFields, Name = name, Properties = properties });
        }

        public void Create(Guid newTenantId, string name, ModelSchemaProperties properties)
        {
            VerifyNotCreated();
            
            RaiseEvent(new ModelSchemaCreated { TenantId = newTenantId, Name = name, Properties = properties });
        }

        public void Update(ModelSchemaProperties properties)
        {
            VerifyCreatedAndNotDeleted();
            
            RaiseEvent(new ModelSchemaUpdated { Properties = properties });
        }

        public void UpdateField(long fieldId, IModelFieldProperties properties)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(new ModelFieldUpdated { FieldId = fieldId, Properties = properties });
        }

        public void HideField(long fieldId)
        {
            VerifyCreatedAndNotDeleted();
            
            RaiseEvent(new ModelFieldHidden { FieldId = fieldId });
        }

        public void ShowField(long fieldId)
        {
            VerifyCreatedAndNotDeleted();
            
            RaiseEvent(new ModelFieldShown { FieldId = fieldId });
        }

        public void DisableField(long fieldId)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(new ModelFieldDisabled { FieldId = fieldId });
        }

        public void EnableField(long fieldId)
        {
            VerifyCreatedAndNotDeleted();
            
            RaiseEvent(new ModelFieldEnabled { FieldId = fieldId });
        }

        public void DeleteField(long fieldId)
        {
            VerifyCreatedAndNotDeleted();
            
            RaiseEvent(new ModelFieldDeleted { FieldId = fieldId });
        }

        public void Delete()
        {
            VerifyCreatedAndNotDeleted();
            
            RaiseEvent(new ModelSchemaDeleted());
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

        protected override void ApplyEvent(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload);
        }
    }
}