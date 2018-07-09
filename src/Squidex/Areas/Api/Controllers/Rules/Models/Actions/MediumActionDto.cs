// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using NJsonSchema.Annotations;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Actions
{
    [JsonSchema("Medium")]
    public class MediumActionDto : RuleActionDto
    {
        /// <summary>
        /// The self issued access token.
        /// </summary>
        [Required]
        public string AccessToken { get; set; }

        /// <summary>
        /// The optional comma separated list of tags.
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// The title, used for the url.
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// The content, either html or markdown.
        /// </summary>
        [Required]
        public string Content { get; set; }

        /// <summary>
        /// The original home of this content, if it was originally published elsewhere.
        /// </summary>
        public string CanonicalUrl { get; set; }

        /// <summary>
        /// Indicates whether the content is markdown or html.
        /// </summary>
        public bool IsHtml { get; set; }

        public override RuleAction ToAction()
        {
            return SimpleMapper.Map(this, new MediumAction());
        }
    }
}
