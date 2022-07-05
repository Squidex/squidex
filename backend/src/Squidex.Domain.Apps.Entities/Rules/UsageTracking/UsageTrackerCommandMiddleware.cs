// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking
{
    public sealed class UsageTrackerCommandMiddleware : ICommandMiddleware
    {
        private readonly IMessageBus messaging;

        public UsageTrackerCommandMiddleware(IMessageBus messaging)
        {
            this.messaging = messaging;
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            switch (context.Command)
            {
                case DeleteRule deleteRule:
                    await messaging.PublishAsync(new UsageTrackingRemove(deleteRule.RuleId));
                    break;
                case CreateRule createRule when createRule.Trigger is UsageTrigger usage:
                    await messaging.PublishAsync(new UsageTrackingAdd(createRule.RuleId, createRule.AppId, usage.Limit, usage.NumDays));
                    break;
                case UpdateRule ruleUpdated when ruleUpdated.Trigger is UsageTrigger usage:
                    await messaging.PublishAsync(new UsageTrackingUpdate(ruleUpdated.RuleId, usage.Limit, usage.NumDays));
                    break;
            }

            await next(context);
        }
    }
}
