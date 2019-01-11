// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;

namespace Squidex.Domain.Apps.Core.HandleRules.Triggers
{
    public sealed class UsageTriggerHandler : RuleTriggerHandler<UsageTrigger, AppUsageExceeded, EnrichedUsageExceededEvent>
    {
        protected override bool Trigger(EnrichedUsageExceededEvent @event, UsageTrigger trigger)
        {
            return @event.Limit == trigger.Limit;
        }
    }
}
