// ==========================================================================
//  SchemaHistoryEventsCreator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Domain.Apps.Read.History;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Read.Schemas
{
    public sealed class SchemaHistoryEventsCreator : HistoryEventsCreatorBase
    {
        public SchemaHistoryEventsCreator(TypeNameRegistry typeNameRegistry)
            : base(typeNameRegistry)
        {
            AddEventMessage<SchemaCreated>(
                "created schema {[Name]}");

            AddEventMessage<SchemaUpdated>(
                "updated schema {[Name]}");

            AddEventMessage<SchemaDeleted>(
                "deleted schema {[Name]}");

            AddEventMessage<SchemaPublished>(
                "published schema {[Name]}");

            AddEventMessage<SchemaUnpublished>(
                "unpublished schema {[Name]}");

            AddEventMessage<SchemaFieldsReordered>(
                "reordered fields of schema {[Name]}");

            AddEventMessage<FieldAdded>(
                "added field {[Field]} to schema {[Name]}");

            AddEventMessage<FieldDeleted>(
                "deleted field {[Field]} from schema {[Name]}");

            AddEventMessage<FieldLocked>(
                "has locked field {[Field]} of schema {[Name]}");

            AddEventMessage<FieldHidden>(
                "has hidden field {[Field]} of schema {[Name]}");

            AddEventMessage<FieldShown>(
                "has shown field {[Field]} of schema {[Name]}");

            AddEventMessage<FieldDisabled>(
                "disabled field {[Field]} of schema {[Name]}");

            AddEventMessage<FieldEnabled>(
                "disabled field {[Field]} of schema {[Name]}");

            AddEventMessage<FieldUpdated>(
                "has updated field {[Field]} of schema {[Name]}");

            AddEventMessage<FieldDeleted>(
                "deleted field {[Field]} of schema {[Name]}");
        }

        protected override Task<HistoryEventToStore> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            if (@event.Payload is SchemaEvent schemaEvent)
            {
                var channel = $"schemas.{schemaEvent.SchemaId.Name}";

                var result = ForEvent(@event.Payload, channel).AddParameter("Name", schemaEvent.SchemaId.Name);

                if (schemaEvent is FieldEvent fieldEvent)
                {
                    result.AddParameter("Field", fieldEvent.FieldId.Name);
                }

                return Task.FromResult(result);
            }

            return Task.FromResult<HistoryEventToStore>(null);
        }
    }
}