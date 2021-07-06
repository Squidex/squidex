// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public sealed class SchemaChangedTriggerHandler : IRuleTriggerHandler
    {
        private readonly IScriptEngine scriptEngine;

        public Type TriggerType => typeof(SchemaChangedTrigger);

        public SchemaChangedTriggerHandler(IScriptEngine scriptEngine)
        {
            this.scriptEngine = scriptEngine;
        }

        public bool Handles(AppEvent appEvent)
        {
            return appEvent is SchemaEvent;
        }

        public async IAsyncEnumerable<EnrichedEvent> CreateEnrichedEventsAsync(Envelope<AppEvent> @event, RuleContext context,
            [EnumeratorCancellation] CancellationToken ct)
        {
            var result = new EnrichedSchemaEvent();

            SimpleMapper.Map(@event.Payload, result);

            switch (@event.Payload)
            {
                case FieldEvent:
                case SchemaPreviewUrlsConfigured:
                case SchemaScriptsConfigured:
                case SchemaUpdated:
                case ParentFieldEvent:
                    result.Type = EnrichedSchemaEventType.Updated;
                    break;
                case SchemaCreated:
                    result.Type = EnrichedSchemaEventType.Created;
                    break;
                case SchemaPublished:
                    result.Type = EnrichedSchemaEventType.Published;
                    break;
                case SchemaUnpublished:
                    result.Type = EnrichedSchemaEventType.Unpublished;
                    break;
                case SchemaDeleted:
                    result.Type = EnrichedSchemaEventType.Deleted;
                    break;
                default:
                    yield break;
            }

            await Task.Yield();

            yield return result;
        }

        public bool Trigger(EnrichedEvent @event, RuleContext context)
        {
            var trigger = (SchemaChangedTrigger)context.Rule.Trigger;

            if (string.IsNullOrWhiteSpace(trigger.Condition))
            {
                return true;
            }

            var vars = new ScriptVars
            {
                ["event"] = @event
            };

            return scriptEngine.Evaluate(vars, trigger.Condition);
        }
    }
}
