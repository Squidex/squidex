// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking
{
    public sealed class UsageTrackerCommandMiddleware : ICommandMiddleware
    {
        private readonly IGrainFactory grainFactory;

        public UsageTrackerCommandMiddleware(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            switch (context.Command)
            {
                case DeleteRule deleteRule:
                    await GetGrain().RemoveTargetAsync(deleteRule.RuleId);
                    break;
                case CreateRule createRule:
                    {
                        if (createRule.Trigger is UsageTrigger usage)
                        {
                            await GetGrain().AddTargetAsync(createRule.RuleId, createRule.AppId, usage.Limit, usage.NumDays);
                        }

                        break;
                    }

                case UpdateRule ruleUpdated:
                    {
                        if (ruleUpdated.Trigger is UsageTrigger usage)
                        {
                            await GetGrain().UpdateTargetAsync(ruleUpdated.RuleId, usage.Limit, usage.NumDays);
                        }

                        break;
                    }
            }

            await next(context);
        }

        private IUsageTrackerGrain GetGrain()
        {
            return grainFactory.GetGrain<IUsageTrackerGrain>(SingleGrain.Id);
        }
    }
}
