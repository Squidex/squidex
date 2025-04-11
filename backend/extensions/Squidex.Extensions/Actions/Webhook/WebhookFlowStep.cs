// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Flows.Steps;
using Squidex.Infrastructure.Reflection;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Squidex.Extensions.Actions.Webhook;

public sealed record WebhookFlowStep : WebhookFlowStepBase, IConvertibleToAction
{
    public RuleAction ToAction()
    {
        return SimpleMapper.Map(this, new WebhookAction());
    }
}
