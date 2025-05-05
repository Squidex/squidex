// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Extensions.Actions.Slack;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Migrations.OldActions;

[Obsolete("Has been replaced by flows.")]
public sealed record SlackAction : RuleAction
{
    [AbsoluteUrl]
    [LocalizedRequired]
    public Uri WebhookUrl { get; set; }

    [LocalizedRequired]
    public string Text { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new SlackFlowStep());
    }
}
