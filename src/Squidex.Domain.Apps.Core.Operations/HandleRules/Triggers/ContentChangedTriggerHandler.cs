// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.HandleRules.Triggers
{
    public sealed class ContentChangedTriggerHandler : RuleTriggerHandler<ContentChangedTriggerV2>
    {
        private readonly IScriptEngine scriptEngine;

        public ContentChangedTriggerHandler(IScriptEngine scriptEngine)
        {
            Guard.NotNull(scriptEngine, nameof(scriptEngine));

            this.scriptEngine = scriptEngine;
        }

        protected override bool Triggers(EnrichedEvent @event, ContentChangedTriggerV2 trigger)
        {
            if (trigger.HandleAll)
            {
                return true;
            }

            if (trigger.Schemas != null && @event is EnrichedSchemaEvent schemaEvent)
            {
                foreach (var schema in trigger.Schemas)
                {
                    if (MatchsSchema(schema, schemaEvent) && MatchsCondition(schema, schemaEvent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool MatchsSchema(ContentChangedTriggerSchemaV2 schema, EnrichedSchemaEvent @event)
        {
            return @event.SchemaId.Id == schema.SchemaId;
        }

        private bool MatchsCondition(ContentChangedTriggerSchemaV2 schema, EnrichedSchemaEvent @event)
        {
            return string.IsNullOrWhiteSpace(schema.Condition) || scriptEngine.Evaluate("event", @event, schema.Condition);
        }
    }
}
