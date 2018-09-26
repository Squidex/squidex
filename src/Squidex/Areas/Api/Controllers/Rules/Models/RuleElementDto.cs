// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class RuleElementDto
    {
        /// <summary>
        /// Describes the action or trigger type.
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// The label for the action or trigger type.
        /// </summary>
        [Required]
        public string Display { get; set; }

        /// <summary>
        /// The color for the icon.
        /// </summary>
        public string IconColor { get; set; }

        /// <summary>
        /// The image for the icon.
        /// </summary>
        public string IconImage { get; set; }

        /// <summary>
        /// The optional link to the product that is integrated.
        /// </summary>
        public string ReadMore { get; set; }
    }
}
