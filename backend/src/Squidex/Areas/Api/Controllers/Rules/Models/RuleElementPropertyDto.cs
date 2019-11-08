// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.HandleRules;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class RuleElementPropertyDto
    {
        /// <summary>
        /// The html editor.
        /// </summary>
        [Required]
        public RuleActionPropertyEditor Editor { get; set; }

        /// <summary>
        /// The name of the editor.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The label to use.
        /// </summary>
        [Required]
        public string Display { get; set; }

        /// <summary>
        /// The optional description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Indicates if the property is formattable.
        /// </summary>
        public bool IsFormattable { get; set; }

        /// <summary>
        /// Indicates if the property is required.
        /// </summary>
        public bool IsRequired { get; set; }
    }
}
