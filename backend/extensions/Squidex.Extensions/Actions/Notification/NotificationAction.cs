// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Notification;

[Obsolete("Has been replaced by flows.")]
public sealed record NotificationAction : RuleAction
{
    [LocalizedRequired]
    public string User { get; set; }

    [LocalizedRequired]
    public string Text { get; set; }

    public string? Url { get; set; }

    public string? Client { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new NotificationFlowStep());
    }
}
