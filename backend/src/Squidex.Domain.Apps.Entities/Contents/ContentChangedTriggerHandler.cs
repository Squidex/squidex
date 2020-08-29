// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentChangedTriggerHandler : RuleTriggerHandler<ContentChangedTriggerV2, ContentEvent, EnrichedContentEvent>
    {
        private readonly IScriptEngine scriptEngine;
        private readonly IContentLoader contentLoader;

        public ContentChangedTriggerHandler(IScriptEngine scriptEngine, IContentLoader contentLoader)
        {
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(contentLoader, nameof(contentLoader));

            this.scriptEngine = scriptEngine;
            this.contentLoader = contentLoader;
        }

        protected override async Task<EnrichedContentEvent?> CreateEnrichedEventAsync(Envelope<ContentEvent> @event)
        {
            var result = new EnrichedContentEvent();

            var content =
                await contentLoader.GetAsync(
                    @event.Payload.AppId.Id,
                    @event.Payload.ContentId,
                    @event.Headers.EventStreamNumber());

            SimpleMapper.Map(content, result);

            switch (@event.Payload)
            {
                case ContentCreated _:
                    result.Type = EnrichedContentEventType.Created;
                    break;
                case ContentDeleted _:
                    result.Type = EnrichedContentEventType.Deleted;
                    break;

                case ContentStatusChanged statusChanged:
                    {
                        switch (statusChanged.Change)
                        {
                            case StatusChange.Published:
                                result.Type = EnrichedContentEventType.Published;
                                break;
                            case StatusChange.Unpublished:
                                result.Type = EnrichedContentEventType.Unpublished;
                                break;
                            default:
                                result.Type = EnrichedContentEventType.StatusChanged;
                                break;
                        }

                        break;
                    }

                case ContentUpdated _:
                    {
                        result.Type = EnrichedContentEventType.Updated;

                        var previousContent =
                            await contentLoader.GetAsync(
                                content.AppId.Id,
                                content.Id,
                                content.Version - 1);

                        result.DataOld = previousContent.Data;
                        break;
                    }
            }

            result.Name = $"{content.SchemaId.Name.ToPascalCase()}{result.Type}";

            return result;
        }

        protected override bool Trigger(ContentEvent @event, ContentChangedTriggerV2 trigger, DomainId ruleId)
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

        protected override bool Trigger(EnrichedContentEvent @event, ContentChangedTriggerV2 trigger)
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

        private static bool MatchsSchema(ContentChangedTriggerSchemaV2 schema, NamedId<DomainId> eventId)
        {
            return eventId.Id == schema.SchemaId;
        }

        private bool MatchsCondition(ContentChangedTriggerSchemaV2 schema, EnrichedSchemaEventBase @event)
        {
            if (string.IsNullOrWhiteSpace(schema.Condition))
            {
                return true;
            }

            var vars = new ScriptVars
            {
                ["event"] = @event
            };

            return scriptEngine.Evaluate(vars, schema.Condition);
        }
    }
}
