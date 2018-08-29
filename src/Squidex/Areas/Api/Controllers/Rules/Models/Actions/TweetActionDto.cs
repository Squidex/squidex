// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using NJsonSchema.Annotations;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Actions
{
    [JsonSchema("Tweet")]
    public sealed class TweetActionDto : RuleActionDto
    {
        /// <summary>
        /// The access token.
        /// </summary>
        [Required]
        public string AccessToken { get; set; }

        /// <summary>
        /// The access secret.
        /// </summary>
        [Required]
        public string AccessSecret { get; set; }

        /// <summary>
        /// The text that is sent as tweet to twitter.
        /// </summary>
        public string Text { get; set; }

        public override RuleAction ToAction()
        {
            return SimpleMapper.Map(this, new TweetAction());
        }
    }
}
