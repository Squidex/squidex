// ==========================================================================
//  AppHistoryEventsCreator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Read.History;
using Squidex.Read.Schemas.Services;
// ReSharper disable InvertIf

namespace Squidex.Read.Schemas
{
    public sealed class SchemaHistoryEventsCreator : HistoryEventsCreatorBase
    {
        private readonly ISchemaProvider schemaProvider;

        public SchemaHistoryEventsCreator(TypeNameRegistry typeNameRegistry, ISchemaProvider schemaProvider)
            : base(typeNameRegistry)
        {
            Guard.NotNull(schemaProvider, nameof(schemaProvider));

            this.schemaProvider = schemaProvider;

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

        protected override async Task<HistoryEventToStore> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            var schemaCreated = @event.Payload as SchemaCreated;

            if (schemaCreated != null)
            {
                string channel = $"schemas.{schemaCreated.Name}";

                return ForEvent(@event.Payload, channel).AddParameter("Name", schemaCreated.Name);
            }
            else
            {
                var schemaEntity = await schemaProvider.FindSchemaByIdAsync(@event.Headers.AggregateId());
                var schemaName = schemaEntity.Label ?? schemaEntity.Name;

                string channel = $"schemas.{schemaName}";

                var result = ForEvent(@event.Payload, channel).AddParameter("Name", schemaName);

                var fieldAdded = @event.Payload as FieldAdded;

                if (fieldAdded != null)
                {
                    result.AddParameter("Field", fieldAdded.Name);
                }
                else
                {
                    var fieldEvent = @event.Payload as FieldEvent;

                    if (fieldEvent != null)
                    {
                        var fieldName = schemaEntity.Schema.Fields.GetOrDefault(fieldEvent.FieldId)?.Name;

                        result.AddParameter("Field", fieldName);
                    }
                }

                return result;
            }
        }
    }
}