// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Old;
using Squidex.Flows.Steps;

namespace Squidex.Extensions.Actions.Webhook;

public sealed record WebhookAction : RuleAction<WebhookStep>
{
    public Uri Url { get; set; }

    public WebhookMethod Method { get; set; }

    public string? Payload { get; set; }

    public string? PayloadType { get; set; }

    public string? Headers { get; set; }

    public string? SharedSecret { get; set; }
}
