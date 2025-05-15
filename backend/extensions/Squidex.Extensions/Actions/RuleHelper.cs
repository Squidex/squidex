// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Flows;
using Squidex.Infrastructure;

namespace Squidex.Extensions.Actions;

public static class RuleHelper
{
    public static bool ShouldDelete(this EnrichedEvent @event, FlowExecutionContext context, string? expression)
    {
        if (!string.IsNullOrWhiteSpace(expression))
        {
            return context.Evaluate(expression, context.Context);
        }

        return IsContentDeletion(@event) || IsAssetDeletion(@event);
    }

    public static bool IsContentDeletion(this EnrichedEvent @event)
    {
        return @event is EnrichedContentEvent { Type: EnrichedContentEventType.Deleted or EnrichedContentEventType.Unpublished };
    }

    public static bool IsAssetDeletion(this EnrichedEvent @event)
    {
        return @event is EnrichedAssetEvent { Type: EnrichedAssetEventType.Deleted };
    }

    public static (string Id, bool IsGenerated) GetOrCreateId(this EnrichedEvent @event)
    {
        if (@event is IEnrichedEntityEvent enrichedEntityEvent)
        {
            return (enrichedEntityEvent.Id.ToString(), false);
        }
        else
        {
            return (DomainId.NewGuid().ToString(), true);
        }
    }
}
