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

namespace Squidex.Domain.Apps.Rules.Action.Webhook
{
    public sealed class WebhookAction : RuleAction
    {
        [AbsoluteUrl]
        [Required]
        [Display(Name = "Url", Description = "he url of the webhook.")]
        public Uri Url { get; set; }

        [Display(Name = "Shared Secret", Description = "The shared secret that is used to calculate the signature.")]
        public string SharedSecret { get; set; }
    }
}
