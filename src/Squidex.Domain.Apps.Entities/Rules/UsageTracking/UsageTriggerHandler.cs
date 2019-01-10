// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking
{
    public sealed class UsageTriggerHandler : RuleTriggerHandler<UsageTrigger, AppEvent, EnrichedUsageExceededEvent>
    {
        private readonly IUsageTrackerGrain usageTrackerGrain;

        public UsageTriggerHandler(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            usageTrackerGrain = grainFactory.GetGrain<IUsageTrackerGrain>(SingleGrain.Id);
        }

        protected override async Task<bool> TriggersAsync(AppEvent @event, UsageTrigger trigger)
        {
            switch (@event)
            {
                case RuleDeleted _:
                    await usageTrackerGrain.RemoveTargetAsync(@event.AppId);
                    break;
                case RuleEnabled _:
                    await usageTrackerGrain.ActivateTargetAsync(@event.AppId);
                    break;
                case RuleDisabled _:
                    await usageTrackerGrain.DeactivateTargetAsync(@event.AppId);
                    break;
                case RuleCreated ruleCreated:
                    if (ruleCreated.Trigger is UsageTrigger createdTrigger)
                    {
                        await usageTrackerGrain.AddTargetAsync(ruleCreated.AppId, createdTrigger.Limit);
                    }

                    break;
                case RuleUpdated ruleUpdated:
                    if (ruleUpdated.Trigger is UsageTrigger updatedTrigger)
                    {
                        await usageTrackerGrain.AddTargetAsync(ruleUpdated.AppId, updatedTrigger.Limit);
                    }

                    break;
            }

            return @event is AppUsageExceeded;
        }

        protected override Task<bool> TriggersAsync(EnrichedUsageExceededEvent @event, UsageTrigger trigger)
        {
            return TaskHelper.True;
        }
    }
}
