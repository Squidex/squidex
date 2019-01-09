// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.HandleRules.Triggers
{
    public sealed class ContentChangedTriggerHandler : RuleTriggerHandler<ContentChangedTriggerV2, ContentEvent, EnrichedContentEvent>
    {
        private readonly IScriptEngine scriptEngine;

        public ContentChangedTriggerHandler(IScriptEngine scriptEngine)
        {
            Guard.NotNull(scriptEngine, nameof(scriptEngine));

            this.scriptEngine = scriptEngine;
        }

        protected override bool Triggers(ContentEvent @event, ContentChangedTriggerV2 trigger)
        {
            if (trigger.HandleAll)
            {
                return true;
            }

            if (trigger.Schemas != null)
            {
                foreach (var schema in trigger.Schemas)
                {
                    if (MatchsSchema(schema, @event.SchemaId))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override bool Triggers(EnrichedContentEvent @event, ContentChangedTriggerV2 trigger)
        {
            if (trigger.HandleAll)
            {
                return true;
            }

            if (trigger.Schemas != null)
            {
                foreach (var schema in trigger.Schemas)
                {
                    if (MatchsSchema(schema, @event.SchemaId) && MatchsCondition(schema, @event))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool MatchsSchema(ContentChangedTriggerSchemaV2 schema, NamedId<Guid> eventId)
        {
            return eventId.Id == schema.SchemaId;
        }

        private bool MatchsCondition(ContentChangedTriggerSchemaV2 schema, EnrichedSchemaEvent @event)
        {
            return string.IsNullOrWhiteSpace(schema.Condition) || scriptEngine.Evaluate("event", @event, schema.Condition);
        }
    }
}
