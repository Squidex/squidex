// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Old;

namespace Squidex.Extensions.Actions.Notification;

public sealed record NotificationAction : RuleAction<NotificationStep>
{
    public string User { get; set; }

    public string Text { get; set; }

    public string? Url { get; set; }

    public string? Client { get; set; }
}
