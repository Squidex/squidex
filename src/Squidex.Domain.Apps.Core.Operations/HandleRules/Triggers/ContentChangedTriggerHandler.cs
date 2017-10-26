// ==========================================================================
//  ContentChangedTriggerHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Core.HandleRules.Triggers
{
    public sealed class ContentChangedTriggerHandler : RuleTriggerHandler<ContentChangedTrigger>
    {
        protected override bool Triggers(Envelope<AppEvent> @event, ContentChangedTrigger trigger)
        {
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

        private static bool MatchsSchema(ContentChangedTriggerSchema webhookSchema, SchemaEvent @event)
        {
            return @event.SchemaId.Id == webhookSchema.SchemaId;
        }

        private static bool MatchsType(ContentChangedTriggerSchema webhookSchema, SchemaEvent @event)
        {
            return
                (webhookSchema.SendCreate && @event is ContentCreated) ||
                (webhookSchema.SendUpdate && @event is ContentUpdated) ||
                (webhookSchema.SendDelete && @event is ContentDeleted) ||
                (webhookSchema.SendPublish && @event is ContentStatusChanged statusChanged && statusChanged.Status == Status.Published);
        }
    }
}
