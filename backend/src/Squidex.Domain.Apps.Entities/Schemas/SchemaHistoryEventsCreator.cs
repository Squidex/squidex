// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public sealed class SchemaHistoryEventsCreator : HistoryEventsCreatorBase
    {
        public SchemaHistoryEventsCreator(TypeNameRegistry typeNameRegistry)
            : base(typeNameRegistry)
        {
            AddEventMessage<SchemaFieldsReordered>(
                T.Get("history.schemas.fieldsReordered"));

            AddEventMessage<SchemaCreated>(
                T.Get("history.schemas.created"));

            AddEventMessage<SchemaUpdated>(
                T.Get("history.schemas.updated"));

            AddEventMessage<SchemaDeleted>(
                T.Get("history.schemas.deleted"));

            AddEventMessage<SchemaPublished>(
                T.Get("history.schemas.published"));

            AddEventMessage<SchemaUnpublished>(
                T.Get("history.schemas.unpublished"));

            AddEventMessage<SchemaFieldsReordered>(
                "reordered fields of schema {[Name]}.");

            AddEventMessage<SchemaScriptsConfigured>(
                T.Get("history.schemas.scriptsConfigured"));

            AddEventMessage<FieldAdded>(
                T.Get("history.schemas.fieldAdded"));

            AddEventMessage<FieldDeleted>(
                T.Get("history.schemas.fieldDeleted"));

            AddEventMessage<FieldLocked>(
                T.Get("history.schemas.fieldLocked"));

            AddEventMessage<FieldHidden>(
                T.Get("history.schemas.fieldHidden"));

            AddEventMessage<FieldShown>(
                T.Get("history.schemas.fieldShown"));

            AddEventMessage<FieldDisabled>(
                T.Get("history.schemas.fieldDisabled"));

            AddEventMessage<FieldEnabled>(
                "disabled field {[Field]} of schema {[Name]}.");

            AddEventMessage<FieldUpdated>(
                T.Get("history.schemas.fieldUpdated"));

            AddEventMessage<FieldDeleted>(
                T.Get("history.schemas.fieldDeleted"));
        }

        protected override Task<HistoryEvent?> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            HistoryEvent? result = null;

            if (@event.Payload is SchemaEvent schemaEvent)
            {
                var channel = $"schemas.{schemaEvent.SchemaId.Id}";

                result = ForEvent(@event.Payload, channel).Param("Name", schemaEvent.SchemaId.Name);

                if (schemaEvent is FieldEvent fieldEvent)
                {
                    result.Param("Field", fieldEvent.FieldId.Name);
                }
            }

            return Task.FromResult(result);
        }
    }
}