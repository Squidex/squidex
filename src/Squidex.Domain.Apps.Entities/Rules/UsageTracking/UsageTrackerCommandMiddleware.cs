// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking
{
    public sealed class UsageTrackerCommandMiddleware : ICommandMiddleware
    {
        private readonly IUsageTrackerGrain usageTrackerGrain;

        public UsageTrackerCommandMiddleware(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            usageTrackerGrain = grainFactory.GetGrain<IUsageTrackerGrain>(SingleGrain.Id);
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            switch (context.Command)
            {
                case DeleteRule deleteRule:
                    await usageTrackerGrain.RemoveTargetAsync(deleteRule.RuleId);
                    break;
                case CreateRule createRule:
                    {
                        if (createRule.Trigger is UsageTrigger usage)
                        {
                            await usageTrackerGrain.AddTargetAsync(createRule.RuleId, createRule.AppId, usage.Limit, usage.NumDays);
                        }

                        break;
                    }

                case UpdateRule ruleUpdated:
                    {
                        if (ruleUpdated.Trigger is UsageTrigger usage)
                        {
                            await usageTrackerGrain.UpdateTargetAsync(ruleUpdated.RuleId, usage.Limit, usage.NumDays);
                        }

                        break;
                    }
            }

            await next();
        }
    }
}
