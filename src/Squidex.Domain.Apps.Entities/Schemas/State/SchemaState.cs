// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Schemas.State
{
    [CollectionName("Schemas")]
    public class SchemaState : DomainObjectState<SchemaState>, ISchemaEntity
    {
        [DataMember]
        public NamedId<Guid> AppId { get; set; }

        [DataMember]
        public long SchemaFieldsTotal { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember]
        public Schema SchemaDef { get; set; }

        protected void On(SchemaCreated @event)
        {
            SchemaDef = @event.Schema;
            SchemaFieldsTotal = @event.Schema.MaxId();

            AppId = @event.AppId;
        }

        protected void On(FieldAdded @event)
        {
            if (@event.ParentFieldId != null)
            {
                var field = @event.Properties.CreateNestedField(@event.FieldId.Id, @event.Name);

                SchemaDef = SchemaDef.UpdateField(@event.ParentFieldId.Id, x => ((ArrayField)x).AddField(field));
            }
            else
            {
                var partitioning = Partitioning.FromString(@event.Partitioning);

                var field = @event.Properties.CreateRootField(@event.FieldId.Id, @event.Name, partitioning);

                SchemaDef = SchemaDef.DeleteField(@event.FieldId.Id);
                SchemaDef = SchemaDef.AddField(field);
            }

            SchemaFieldsTotal = Math.Max(SchemaFieldsTotal, @event.FieldId.Id);
        }

        protected void On(SchemaCategoryChanged @event)
        {
            SchemaDef = SchemaDef.ChangeCategory(@event.Name);
        }

        protected void On(SchemaPreviewUrlsConfigured @event)
        {
            SchemaDef = SchemaDef.ConfigurePreviewUrls(@event.PreviewUrls);
        }

        protected void On(SchemaScriptsConfigured @event)
        {
            SchemaDef = SchemaDef.ConfigureScripts(@event.Scripts);
        }

        protected void On(SchemaPublished @event)
        {
            SchemaDef = SchemaDef.Publish();
        }

        protected void On(SchemaUnpublished @event)
        {
            SchemaDef = SchemaDef.Unpublish();
        }

        protected void On(SchemaUpdated @event)
        {
            SchemaDef = SchemaDef.Update(@event.Properties);
        }

        protected void On(SchemaFieldsReordered @event)
        {
            SchemaDef = SchemaDef.ReorderFields(@event.FieldIds, @event.ParentFieldId?.Id);
        }

        protected void On(FieldUpdated @event)
        {
            SchemaDef = SchemaDef.UpdateField(@event.FieldId.Id, @event.Properties, @event.ParentFieldId?.Id);
        }

        protected void On(FieldLocked @event)
        {
            SchemaDef = SchemaDef.LockField(@event.FieldId.Id, @event.ParentFieldId?.Id);
        }

        protected void On(FieldDisabled @event)
        {
            SchemaDef = SchemaDef.DisableField(@event.FieldId.Id, @event.ParentFieldId?.Id);
        }

        protected void On(FieldEnabled @event)
        {
            SchemaDef = SchemaDef.EnableField(@event.FieldId.Id, @event.ParentFieldId?.Id);
        }

        protected void On(FieldHidden @event)
        {
            SchemaDef = SchemaDef.HideField(@event.FieldId.Id, @event.ParentFieldId?.Id);
        }

        protected void On(FieldShown @event)
        {
            SchemaDef = SchemaDef.ShowField(@event.FieldId.Id, @event.ParentFieldId?.Id);
        }

        protected void On(FieldDeleted @event)
        {
            SchemaDef = SchemaDef.DeleteField(@event.FieldId.Id, @event.ParentFieldId?.Id);
        }

        protected void On(SchemaDeleted @event)
        {
            IsDeleted = true;
        }

        public SchemaState Apply(Envelope<IEvent> @event)
        {
            var payload = (SquidexEvent)@event.Payload;

            return Clone().Update(payload, @event.Headers, r => r.DispatchAction(payload));
        }
    }
}
