// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Domain.Apps.Rules.Action.Twitter
{
    public sealed class TweetAction : RuleAction
    {
        [Required]
        [Display(Name = "Access Token", Description = " The generated access token.")]
        public string AccessToken { get; set; }

        [Required]
        [Display(Name = "Access Secret", Description = " The generated access secret.")]
        public string AccessSecret { get; set; }

        [Required]
        [Display(Name = "Text", Description = "The text that is sent as tweet to twitter.")]
        public string Text { get; set; }
    }
}
