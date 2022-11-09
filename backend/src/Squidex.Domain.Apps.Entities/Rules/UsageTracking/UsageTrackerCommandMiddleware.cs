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

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking;

public sealed class UsageTrackerCommandMiddleware : ICommandMiddleware
{
    private readonly IMessageBus messaging;

    public UsageTrackerCommandMiddleware(IMessageBus messaging)
    {
        this.messaging = messaging;
    }

    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        switch (context.Command)
        {
            case DeleteRule deleteRule:
                await messaging.PublishAsync(new UsageTrackingRemove(deleteRule.RuleId), ct: default);
                break;
            case CreateRule { Trigger: UsageTrigger usage } createRule:
                await messaging.PublishAsync(new UsageTrackingAdd(createRule.RuleId, createRule.AppId, usage.Limit, usage.NumDays), ct: default);
                break;
            case UpdateRule { Trigger: UsageTrigger usage } ruleUpdated:
                await messaging.PublishAsync(new UsageTrackingUpdate(ruleUpdated.RuleId, usage.Limit, usage.NumDays), ct: default);
                break;
        }

        await next(context, ct);
    }
}
