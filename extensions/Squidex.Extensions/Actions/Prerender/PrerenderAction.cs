// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Extensions.Actions.Prerender
{
    [RuleActionHandler(typeof(PrerenderActionHandler))]
    [RuleAction(
        IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M2.073 17.984l8.646-5.36v-1.787L.356 17.325v1.318l10.363 6.488v-1.787zM29.927 17.984l-8.646-5.36v-1.787l10.363 6.488v1.318l-10.363 6.488v-1.787zM18.228 6.693l-6.276 19.426 1.656.548 6.276-19.426z'/></svg>",
        IconColor = "#2c3e50",
        Display = "Recache URL",
        Description = "Recache a certain URL or cache a URL for the first time.",
        ReadMore = "https://prerender.io")]
    public sealed class PrerenderAction : RuleAction
    {
        [Required]
        [Display(Name = "Token", Description = "The prerender token from your account.")]
        public string Token { get; set; }

        [Required]
        [Display(Name = "Url", Description = "The url to recache.")]
        public string Url { get; set; }
    }
}
