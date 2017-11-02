// ==========================================================================
//  WebhookActionDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NJsonSchema.Annotations;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Controllers.Api.Rules.Models.Actions
{
    [JsonSchema("Webhook")]
    public sealed class WebhookActionDto : RuleActionDto
    {
        /// <summary>
        /// The url of the rule.
        /// </summary>
        [Required]
        public Uri Url { get; set; }

        /// <summary>
        /// The shared secret that is used to calculate the signature.
        /// </summary>
        public string SharedSecret { get; set; }

        public override RuleAction ToAction()
        {
            return SimpleMapper.Map(this, new WebhookAction());
        }
    }
}
