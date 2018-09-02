// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Rules.Actions.Discourse
{
    [RuleActionHandler(typeof(DiscourseActionHandler))]
    [RuleAction(Description = "")]
    public sealed class DiscourseAction : RuleAction
    {
        [AbsoluteUrl]
        [Required]
        [Display(Name = "Url", Description = "he url to the discourse server.")]
        public Uri Url { get; set; }

        [Required]
        [Display(Name = "Api Key", Description = "The api key.")]
        public string ApiKey { get; set; }

        [Required]
        [Display(Name = "Text", Description = "The text as markdown.")]
        public string Text { get; set; }

        [Display(Name = "Title", Description = "The optional title when creating new topics.")]
        public string Title { get; set; }

        [Display(Name = "Topic", Description = "The optional topic id.")]
        public int? Topic { get; set; }

        [Display(Name = "Category", Description = "The optional category id.")]
        public int? Category { get; set; }
    }
}
