﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public sealed class SchemaChangedTriggerHandler : RuleTriggerHandler<SchemaChangedTrigger, SchemaEvent, EnrichedSchemaEvent>
    {
        private readonly IScriptEngine scriptEngine;

        public SchemaChangedTriggerHandler(IScriptEngine scriptEngine)
        {
            Guard.NotNull(scriptEngine);

            this.scriptEngine = scriptEngine;
        }

        protected override Task<EnrichedSchemaEvent?> CreateEnrichedEventAsync(Envelope<SchemaEvent> @event)
        {
            EnrichedSchemaEvent? result = new EnrichedSchemaEvent();

            SimpleMapper.Map(@event.Payload, result);

            switch (@event.Payload)
            {
                case FieldEvent _:
                case SchemaPreviewUrlsConfigured _:
                case SchemaScriptsConfigured _:
                case SchemaUpdated _:
                case ParentFieldEvent _:
                    result.Type = EnrichedSchemaEventType.Updated;
                    break;
                case SchemaCreated _:
                    result.Type = EnrichedSchemaEventType.Created;
                    break;
                case SchemaPublished _:
                    result.Type = EnrichedSchemaEventType.Published;
                    break;
                case SchemaUnpublished _:
                    result.Type = EnrichedSchemaEventType.Unpublished;
                    break;
                case SchemaDeleted _:
                    result.Type = EnrichedSchemaEventType.Deleted;
                    break;
                default:
                    result = null;
                    break;
            }

            if (result != null)
            {
                result.Name = $"Schema{result.Type}";
            }

            return Task.FromResult(result);
        }

        protected override bool Trigger(EnrichedSchemaEvent @event, SchemaChangedTrigger trigger)
        {
            if (string.IsNullOrWhiteSpace(trigger.Condition))
            {
                return true;
            }

            var context = new ScriptContext
            {
                ["event"] = @event
            };

            return scriptEngine.Evaluate(context, trigger.Condition);
        }
    }
}
