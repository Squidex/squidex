// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Old;

namespace Squidex.Extensions.Actions.Slack;

public sealed record SlackAction : RuleAction<SlackStep>
{
    public Uri WebhookUrl { get; set; }

    public string Text { get; set; }
}
