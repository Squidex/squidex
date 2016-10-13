// ==========================================================================
//  SchemaDomainObject.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Core.Schemas;
using PinkParrot.Events.Schemas;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS;
using PinkParrot.Infrastructure.CQRS.Events;
using PinkParrot.Infrastructure.Dispatching;

namespace PinkParrot.Write.Schemas
{
    public class SchemaDomainObject : DomainObject, IAppAggregate
    {
        private readonly FieldRegistry registry;
        private Guid appId;
        private bool isDeleted;
        private long totalFields;
        private Schema schema;

        public Schema Schema
        {
            get { return schema; }
        }

        public Guid AppId
        {
            get { return appId; }
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

            schema = schema.AddOrUpdateField(registry.CreateField(@event.FieldId, @event.Name, @event.Properties));
        }

        public void On(SchemaCreated @event)
        {
            appId = @event.AppId;

            schema = Schema.Create(@event.Name, @event.Properties);
        }

        public void On(FieldUpdated @event)
        {
            schema = schema.UpdateField(@event.FieldId, @event.Properties);
        }

        public void On(FieldHidden @event)
        {
            schema = schema.HideField(@event.FieldId);
        }

        public void On(FieldShown @event)
        {
            schema = schema.ShowField(@event.FieldId);
        }

        public void On(FieldDisabled @event)
        {
            schema = schema.DisableField(@event.FieldId);
        }

        public void On(FieldEnabled @event)
        {
            schema = schema.EnableField(@event.FieldId);
        }

        public void On(SchemaUpdated @event)
        {
            schema = schema.Update(@event.Properties);
        }

        public void On(FieldDeleted @event)
        {
            schema = schema.DeleteField(@event.FieldId);
        }

        public void On(SchemaDeleted @event)
        {
            isDeleted = false;
        }

        public void AddField(string name, FieldProperties properties)
        {
            VerifyCreatedAndNotDeleted();
            
            RaiseEvent(new FieldAdded { FieldId = ++totalFields, Name = name, Properties = properties });
        }

        public void Create(Guid newAppId, string name, SchemaProperties properties)
        {
            VerifyNotCreated();
            
            RaiseEvent(new SchemaCreated { AppId = newAppId, Name = name, Properties = properties });
        }

        public void Update(SchemaProperties properties)
        {
            VerifyCreatedAndNotDeleted();
            
            RaiseEvent(new SchemaUpdated { Properties = properties });
        }

        public void UpdateField(long fieldId, FieldProperties properties)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(new FieldUpdated { FieldId = fieldId, Properties = properties });
        }

        public void HideField(long fieldId)
        {
            VerifyCreatedAndNotDeleted();
            
            RaiseEvent(new FieldHidden { FieldId = fieldId });
        }

        public void ShowField(long fieldId)
        {
            VerifyCreatedAndNotDeleted();
            
            RaiseEvent(new FieldShown { FieldId = fieldId });
        }

        public void DisableField(long fieldId)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(new FieldDisabled { FieldId = fieldId });
        }

        public void EnableField(long fieldId)
        {
            VerifyCreatedAndNotDeleted();
            
            RaiseEvent(new FieldEnabled { FieldId = fieldId });
        }

        public void DeleteField(long fieldId)
        {
            VerifyCreatedAndNotDeleted();
            
            RaiseEvent(new FieldDeleted { FieldId = fieldId });
        }

        public void Delete()
        {
            VerifyCreatedAndNotDeleted();
            
            RaiseEvent(new SchemaDeleted());
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