// ==========================================================================
//  AppHistoryEventsCreator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Events;
using Squidex.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Read.History;

// ReSharper disable InvertIf

namespace Squidex.Read.Schemas
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

            AddEventMessage<FieldAdded>(
                "added field {[Field]} to schema {[Name]}");

            AddEventMessage<FieldDeleted>(
                "deleted field {[Field]} from schema {[Name]}");

            AddEventMessage<FieldDisabled>(
                "disabled field {[Field]} of schema {[Name]}");

            AddEventMessage<FieldEnabled>(
                "disabled field {[Field]} of schema {[Name]}");

            AddEventMessage<FieldHidden>(
                "has hidden field {[Field]} of schema {[Name]}");

            AddEventMessage<FieldShown>(
                "has shown field {[Field]} of schema {[Name]}");

            AddEventMessage<FieldUpdated>(
                "has updated field {[Field]} of schema {[Name]}");

            AddEventMessage<FieldDeleted>(
                "deleted field {[Field]} of schema {[Name]}");
        }

        protected override Task<HistoryEventToStore> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            var schemaEvent = @event.Payload as SchemaEvent;

            if (schemaEvent == null)
            {
                return Task.FromResult<HistoryEventToStore>(null);
            }

            string channel = $"schemas.{schemaEvent.SchemaId.Name}";

            var result = ForEvent(@event.Payload, channel).AddParameter("Name", schemaEvent.SchemaId.Name);

            var fieldEvent = schemaEvent as FieldEvent;

            if (fieldEvent != null)
            {
                result.AddParameter("Field", fieldEvent.FieldId.Name);
            }

            return Task.FromResult(result);
        }
    }
}