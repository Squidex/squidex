// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Domain.Apps.Rules.Action.Medium
{
    public sealed class MediumAction : RuleAction
    {
        [Required]
        [Display(Name = "Access Token", Description = "The self issued access token.")]
        public string AccessToken { get; set; }

        [Required]
        [Display(Name = "Title", Description = "The title, used for the url.")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Content", Description = "The content, either html or markdown.")]
        public string Content { get; set; }

        [Display(Name = "Canonical Url", Description = "The original home of this content, if it was originally published elsewhere.")]
        public string CanonicalUrl { get; set; }

        [Display(Name = "Tags", Description = "The optional comma separated list of tags.")]
        public string Tags { get; set; }

        [Display(Name = "Is Html", Description = "Indicates whether the content is markdown or html.")]
        public bool IsHtml { get; set; }
    }
}
