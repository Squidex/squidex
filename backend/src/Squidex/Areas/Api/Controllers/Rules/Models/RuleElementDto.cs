// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Infrastructure.Reflection;

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
        /// Optional title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// The color for the icon.
        /// </summary>
        public string? IconColor { get; set; }

        /// <summary>
        /// The image for the icon.
        /// </summary>
        public string? IconImage { get; set; }

        /// <summary>
        /// The optional link to the product that is integrated.
        /// </summary>
        public string? ReadMore { get; set; }

        /// <summary>
        /// The properties.
        /// </summary>
        [Required]
        public RuleElementPropertyDto[] Properties { get; set; }

        public static RuleElementDto FromDefinition(RuleActionDefinition definition)
        {
            var result = SimpleMapper.Map(definition, new RuleElementDto());

            result.Properties = definition.Properties.Select(x => SimpleMapper.Map(x, new RuleElementPropertyDto())).ToArray();

            return result;
        }
    }
}
