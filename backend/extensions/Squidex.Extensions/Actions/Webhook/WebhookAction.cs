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

namespace Squidex.Extensions.Actions.Webhook;

[Obsolete("Has been replaced by flows.")]
public sealed record WebhookAction : RuleAction
{
    [LocalizedRequired]
    public Uri Url { get; set; }

    [LocalizedRequired]
    public WebhookMethod Method { get; set; }

    public string? Payload { get; set; }

    public string? PayloadType { get; set; }

    public string? Headers { get; set; }

    public string? SharedSecret { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new WebhookFlowStep());
    }
}
