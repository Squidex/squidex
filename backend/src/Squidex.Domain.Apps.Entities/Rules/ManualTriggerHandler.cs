// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public sealed class ManualTriggerHandler : RuleTriggerHandler<ManualTrigger, RuleManuallyTriggered, EnrichedManualEvent>
    {
        protected override Task<EnrichedManualEvent?> CreateEnrichedEventAsync(Envelope<RuleManuallyTriggered> @event)
        {
            var result = new EnrichedManualEvent
            {
                Name = "Manual"
            };

            return Task.FromResult<EnrichedManualEvent?>(result);
        }

        protected override bool Trigger(EnrichedManualEvent @event, ManualTrigger trigger)
        {
            return true;
        }
    }
}
