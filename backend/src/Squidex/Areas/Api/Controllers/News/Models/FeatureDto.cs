// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.Api.Controllers.News.Models
{
    public sealed class FeatureDto
    {
        /// <summary>
        /// The name of the feature.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The description text.
        /// </summary>
        [Required]
        public string Text { get; set; }
    }
}
