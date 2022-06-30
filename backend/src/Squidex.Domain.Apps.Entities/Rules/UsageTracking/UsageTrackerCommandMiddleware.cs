// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MassTransit;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking
{
    public sealed class UsageTrackerCommandMiddleware : ICommandMiddleware
    {
        private readonly IBus bus;

        public UsageTrackerCommandMiddleware(IBus bus)
        {
            this.bus = bus;
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            switch (context.Command)
            {
                case DeleteRule deleteRule:
                    await bus.Publish(new UsageTrackingRemove(deleteRule.RuleId));
                    break;
                case CreateRule createRule:
                    {
                        if (createRule.Trigger is UsageTrigger usage)
                        {
                            await bus.Publish(new UsageTrackingAdd(createRule.RuleId, createRule.AppId, usage.Limit, usage.NumDays));
                        }

                        break;
                    }

                case UpdateRule ruleUpdated:
                    {
                        if (ruleUpdated.Trigger is UsageTrigger usage)
                        {
                            await bus.Publish(new UsageTrackingUpdate(ruleUpdated.RuleId, usage.Limit, usage.NumDays));
                        }

                        break;
                    }
            }

            await next(context);
        }
    }
}
