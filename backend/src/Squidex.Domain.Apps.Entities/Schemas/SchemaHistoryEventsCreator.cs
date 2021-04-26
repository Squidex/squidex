// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public sealed class SchemaHistoryEventsCreator : HistoryEventsCreatorBase
    {
        public SchemaHistoryEventsCreator(TypeNameRegistry typeNameRegistry)
            : base(typeNameRegistry)
        {
            AddEventMessage<SchemaFieldsReordered>(
                "history.schemas.fieldsReordered");

            AddEventMessage<SchemaCreated>(
                "history.schemas.created");

            AddEventMessage<SchemaUpdated>(
                "history.schemas.updated");

            AddEventMessage<SchemaDeleted>(
                "history.schemas.deleted");

            AddEventMessage<SchemaPublished>(
                "history.schemas.published");

            AddEventMessage<SchemaUnpublished>(
                "history.schemas.unpublished");

            AddEventMessage<SchemaFieldsReordered>(
                "history.schemas.fieldsReordered");

            AddEventMessage<SchemaScriptsConfigured>(
                "history.schemas.scriptsConfigured");

            AddEventMessage<FieldAdded>(
                "history.schemas.fieldAdded");

            AddEventMessage<FieldDeleted>(
                "history.schemas.fieldDeleted");

            AddEventMessage<FieldLocked>(
                "history.schemas.fieldLocked");

            AddEventMessage<FieldHidden>(
                "history.schemas.fieldHidden");

            AddEventMessage<FieldShown>(
                "history.schemas.fieldShown");

            AddEventMessage<FieldDisabled>(
                "history.schemas.fieldDisabled");

            AddEventMessage<FieldEnabled>(
                "history.schemas.fieldDisabled");

            AddEventMessage<FieldUpdated>(
                "history.schemas.fieldUpdated");

            AddEventMessage<FieldDeleted>(
                "history.schemas.fieldDeleted");
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