// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.HandleRules.Triggers
{
    public sealed class ContentChangedTriggerHandler : RuleTriggerHandler<ContentChangedTrigger>
    {
        protected override bool Triggers(Envelope<AppEvent> @event, ContentChangedTrigger trigger)
        {
            if (trigger.HandleAll)
            {
                return true;
            }

            if (trigger.Schemas != null && @event.Payload is SchemaEvent schemaEvent)
            {
                foreach (var schema in trigger.Schemas)
                {
                    if (MatchsSchema(schema, schemaEvent) && MatchsType(schema, schemaEvent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool MatchsSchema(ContentChangedTriggerSchema schema, SchemaEvent @event)
        {
            return @event.SchemaId.Id == schema.SchemaId;
        }

        private static bool MatchsType(ContentChangedTriggerSchema schema, SchemaEvent @event)
        {
            return
                (schema.SendCreate && @event is ContentCreated) ||
                (schema.SendUpdate && @event is ContentUpdated) ||
                (schema.SendDelete && @event is ContentDeleted) ||
                (schema.SendPublish && @event is ContentStatusChanged statusChanged && statusChanged.Status == Status.Published);
        }
    }
}
