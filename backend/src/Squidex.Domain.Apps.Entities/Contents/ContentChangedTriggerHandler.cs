// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentChangedTriggerHandler : RuleTriggerHandler<ContentChangedTriggerV2, ContentEvent, EnrichedContentEvent>, IRuleEventFormatter
    {
        private readonly IScriptEngine scriptEngine;
        private readonly IContentLoader contentLoader;
        private readonly ILocalCache localCache;

        public ContentChangedTriggerHandler(IScriptEngine scriptEngine, IContentLoader contentLoader, ILocalCache localCache)
        {
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(contentLoader, nameof(contentLoader));
            Guard.NotNull(localCache, nameof(localCache));

            this.scriptEngine = scriptEngine;
            this.contentLoader = contentLoader;
            this.localCache = localCache;
        }

        public (bool Match, ValueTask<string?>) Format(EnrichedEvent @event, object value, string[] path)
        {
            if (string.Equals(path[0], "data", StringComparison.OrdinalIgnoreCase) &&
                value is JsonArray array &&
                array.Count > 0 &&
                array[0] is JsonString s &&
                Guid.TryParse(s.Value, out var referenceId))
            {
                return (true, GetReferenceValueAsync(referenceId, path));
            }

            return default;
        }

        private async ValueTask<string?> GetReferenceValueAsync(Guid referenceId, string[] path)
        {
            var reference = await GetContentFromCacheAsync(referenceId);

            var (result, remaining) = RuleVariable.GetValue(reference, path);

            if (remaining.Length == 0)
            {
                return result?.ToString();
            }

            return default;
        }

        private Task<IContentEntity> GetContentFromCacheAsync(Guid referenceId)
        {
            var cacheKey = $"FORMAT_REFERENCE_{referenceId}";

            return localCache.GetOrCreate(cacheKey, () => contentLoader.GetAsync(referenceId));
        }

        protected override async Task<EnrichedContentEvent?> CreateEnrichedEventAsync(Envelope<ContentEvent> @event)
        {
            var result = new EnrichedContentEvent();

            var content =
                await contentLoader.GetAsync(
                    @event.Headers.AggregateId(),
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
                                content.Id,
                                content.Version - 1);

                        result.DataOld = previousContent.Data;
                        break;
                    }
            }

            result.Name = $"{content.SchemaId.Name.ToPascalCase()}{result.Type}";

            return result;
        }

        protected override bool Trigger(ContentEvent @event, ContentChangedTriggerV2 trigger, Guid ruleId)
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

        private static bool MatchsSchema(ContentChangedTriggerSchemaV2 schema, NamedId<Guid> eventId)
        {
            return eventId.Id == schema.SchemaId;
        }

        private bool MatchsCondition(ContentChangedTriggerSchemaV2 schema, EnrichedSchemaEventBase @event)
        {
            if (string.IsNullOrWhiteSpace(schema.Condition))
            {
                return true;
            }

            var context = new ScriptContext
            {
                ["event"] = @event
            };

            return scriptEngine.Evaluate(context, schema.Condition);
        }
    }
}
