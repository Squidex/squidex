// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NJsonSchema.Annotations;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Actions
{
    [JsonSchema("Slack")]
    public sealed class SlackActionDto : RuleActionDto
    {
        /// <summary>
        /// The slack webhook url.
        /// </summary>
        [Required]
        public Uri WebhookUrl { get; set; }

        /// <summary>
        /// The text that is sent as message to slack.
        /// </summary>
        public string Text { get; set; }

        public override RuleAction ToAction()
        {
            return SimpleMapper.Map(this, new SlackAction());
        }
    }
}
