// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Text;

#pragma warning disable SA1013 // Closing braces should be spaced correctly

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class ContentChangedTriggerHandler : IRuleTriggerHandler, ISubscriptionEventCreator
{
    private readonly IScriptEngine scriptEngine;
    private readonly IContentLoader contentLoader;
    private readonly IContentRepository contentRepository;

    public bool CanCreateSnapshotEvents => true;

    public Type TriggerType => typeof(ContentChangedTriggerV2);

    public bool Handles(AppEvent appEvent)
    {
        return appEvent is ContentEvent;
    }

    public ContentChangedTriggerHandler(
        IScriptEngine scriptEngine,
        IContentLoader contentLoader,
        IContentRepository contentRepository)
    {
        this.scriptEngine = scriptEngine;
        this.contentLoader = contentLoader;
        this.contentRepository = contentRepository;
    }

    public async IAsyncEnumerable<EnrichedEvent> CreateSnapshotEventsAsync(RuleContext context,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var trigger = (ContentChangedTriggerV2)context.Rule.Trigger;

        var schemaIds =
            trigger.Schemas?.Count > 0 ?
            trigger.Schemas.Select(x => x.SchemaId).Distinct().ToHashSet() :
            null;

        await foreach (var content in contentRepository.StreamAll(context.AppId.Id, schemaIds, SearchScope.All, ct))
        {
            var result = new EnrichedContentEvent
            {
                Type = EnrichedContentEventType.Created
            };

            SimpleMapper.Map(content, result);

            result.Actor = content.LastModifiedBy;
            result.Name = $"ContentQueried({content.SchemaId.Name.ToPascalCase()})";

            yield return result;
        }
    }

    public async IAsyncEnumerable<EnrichedEvent> CreateEnrichedEventsAsync(Envelope<AppEvent> @event, RulesContext context,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var enrichedEvent = await CreateEnrichedEventsCoreAsync(@event, ct);

        yield return enrichedEvent;

        if (!context.AllowExtraEvents ||
            context.MaxEvents == null ||
            context.MaxEvents <= 0)
        {
            yield break;
        }

        // When the content has just been created, it cannot be referenced by another content. Therefore we can skip it.1
        if (enrichedEvent.Type == EnrichedContentEventType.Created)
        {
            yield break;
        }

        var allTriggers = context.Rules.Select(x => x.Value.Trigger).OfType<ContentChangedTriggerV2>();

        // This method is only called once per event, therefore we check all rules.
        if (!allTriggers.Any(t => MatchesAnySchema(t.ReferencedSchemas, enrichedEvent)))
        {
            yield break;
        }

        var take = context.MaxEvents.Value;

        await foreach (var content in contentRepository.StreamReferencing(context.AppId.Id, enrichedEvent.Id, take, SearchScope.All, ct))
        {
            var result = new EnrichedContentEvent
            {
                Type = EnrichedContentEventType.ReferenceUpdated
            };

            SimpleMapper.Map(content, result);

            result.Actor = content.LastModifiedBy;
            result.Name = $"{content.SchemaId.Name.ToPascalCase()}ReferenceUpdated";

            yield return result;
        }
    }

    public async ValueTask<EnrichedEvent?> CreateEnrichedEventsAsync(Envelope<AppEvent> @event,
        CancellationToken ct)
    {
        return await CreateEnrichedEventsCoreAsync(@event, ct);
    }

    private async ValueTask<EnrichedContentEvent> CreateEnrichedEventsCoreAsync(Envelope<AppEvent> @event,
        CancellationToken ct)
    {
        var contentEvent = (ContentEvent)@event.Payload;

        var result = new EnrichedContentEvent();

        var content =
            await contentLoader.GetAsync(
                contentEvent.AppId.Id,
                contentEvent.ContentId,
                @event.Headers.EventStreamNumber(),
                ct);

        if (content != null)
        {
            SimpleMapper.Map(content, result);
        }

        // Use the concrete event to map properties that are not part of app event.
        SimpleMapper.Map(contentEvent, result);

        // This property has another name, so we cannot use the simple mapper.
        result.Id = contentEvent.ContentId;

        switch (@event.Payload)
        {
            case ContentCreated:
                result.Type = EnrichedContentEventType.Created;
                break;
            case ContentDeleted:
                result.Type = EnrichedContentEventType.Deleted;
                break;
            case ContentStatusChanged { Change: StatusChange.Published }:
                result.Type = EnrichedContentEventType.Published;
                break;
            case ContentStatusChanged { Change: StatusChange.Unpublished }:
                result.Type = EnrichedContentEventType.Unpublished;
                break;
            case ContentStatusChanged { Change: StatusChange.Change }:
                result.Type = EnrichedContentEventType.StatusChanged;
                break;
            case ContentUpdated:
                {
                    result.Type = EnrichedContentEventType.Updated;

                    if (content != null)
                    {
                        var previousContent =
                            await contentLoader.GetAsync(
                                content.AppId.Id,
                                content.Id,
                                content.Version - 1,
                                ct);

                        if (previousContent != null)
                        {
                            result.DataOld = previousContent.Data;
                        }
                    }

                    break;
                }
        }

        return result;
    }

    public string? GetName(AppEvent @event)
    {
        switch (@event)
        {
            case ContentCreated e:
                return $"{e.SchemaId.Name.ToPascalCase()}Created";
            case ContentDeleted e:
                return $"{e.SchemaId.Name.ToPascalCase()}Deleted";
            case ContentStatusChanged { Change: StatusChange.Published } e:
                return $"{e.SchemaId.Name.ToPascalCase()}Published";
            case ContentStatusChanged { Change: StatusChange.Unpublished } e:
                return $"{e.SchemaId.Name.ToPascalCase()}Unpublished";
            case ContentStatusChanged { Change: StatusChange.Change } e:
                return $"{e.SchemaId.Name.ToPascalCase()}StatusChanged";
            case ContentUpdated e:
                return $"{e.SchemaId.Name.ToPascalCase()}Updated";
        }

        return null;
    }

    public bool Trigger(Envelope<AppEvent> @event, RuleTrigger trigger)
    {
        var typed = (ContentChangedTriggerV2)trigger;

        if (typed.HandleAll)
        {
            return true;
        }

        // Also check for the referenced schemas, because it is needed to query the references.
        return MatchesAnySchema(typed.Schemas, @event.Payload) || MatchesAnySchema(typed.ReferencedSchemas, @event.Payload);
    }

    public bool Trigger(EnrichedEvent @event, RuleTrigger trigger)
    {
        var typed = (ContentChangedTriggerV2)trigger;

        if (typed.HandleAll)
        {
            return true;
        }

        // Only check for the actual schemas, not references schemas as they have already been queried.
        return MatchesAnySchema(typed.Schemas, @event);
    }

    private bool MatchesAnySchema(ReadonlyList<SchemaCondition>? schemas, EnrichedEvent @event)
    {
        if (schemas == null)
        {
            return false;
        }

        var contentEvent = (EnrichedContentEvent)@event;

        foreach (var schema in schemas)
        {
            // Check for the conditions once to improve performance.
            if (MatchsSchema(schema, contentEvent.SchemaId) && MatchsCondition(schema, contentEvent))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesAnySchema(ReadonlyList<SchemaCondition>? schemas, AppEvent @event)
    {
        if (schemas == null)
        {
            return false;
        }

        var contentEvent = (ContentEvent)@event;

        foreach (var schema in schemas)
        {
            // Check for the conditions once to improve performance.
            if (MatchsSchema(schema, contentEvent.SchemaId))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchsSchema(SchemaCondition? schema, NamedId<DomainId> schemaId)
    {
        return schemaId != null && schemaId.Id == schema?.SchemaId;
    }

    private bool MatchsCondition(SchemaCondition schema, EnrichedSchemaEventBase @event)
    {
        if (string.IsNullOrWhiteSpace(schema.Condition))
        {
            return true;
        }

        // Script vars are just wrappers over dictionaries for better performance.
        var vars = new EventScriptVars
        {
            Event = @event
        };

        return scriptEngine.Evaluate(vars, schema.Condition);
    }
}
