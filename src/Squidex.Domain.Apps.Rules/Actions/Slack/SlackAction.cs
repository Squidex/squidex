// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Rules.Action.Slack
{
    public sealed class SlackAction : RuleAction
    {
        [AbsoluteUrl]
        [Required]
        [Display(Name = "Webhook Url", Description = "The slack webhook url.")]
        public Uri WebhookUrl { get; set; }

        [Required]
        [Display(Name = "Text", Description = "The text that is sent as message to slack.")]
        public string Text { get; set; }
    }
}
