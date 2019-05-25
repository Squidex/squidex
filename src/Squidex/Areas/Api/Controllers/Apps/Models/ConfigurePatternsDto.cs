// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class ConfigurePatternsDto
    {
        /// <summary>
        /// The list of patterns.
        /// </summary>
        [Required]
        public AppPatternDto[] Patterns { get; set; }

        public ConfigurePatterns ToConfigureCommand()
        {
            return new ConfigurePatterns
            {
                Patterns = Patterns?.Select(p => SimpleMapper.Map(p, new UpsertAppPattern())).ToArray()
            };
        }
    }
}
