// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Flows;

namespace Squidex.Extensions.Actions;

public static class RuleHelper
{
    public static bool ShouldDelete(this FlowExecutionContext executionContext, RuleFlowContext context, string? expression)
    {
        if (!string.IsNullOrWhiteSpace(expression))
        {
            return executionContext.Evaluate(expression, context);
        }

        return IsContentDeletion(context.Event) || IsAssetDeletion(context.Event);
    }

    public static bool IsContentDeletion(this EnrichedEvent @event)
    {
        return @event is EnrichedContentEvent { Type: EnrichedContentEventType.Deleted or EnrichedContentEventType.Unpublished };
    }

    public static bool IsAssetDeletion(this EnrichedEvent @event)
    {
        return @event is EnrichedAssetEvent { Type: EnrichedAssetEventType.Deleted };
    }
}
