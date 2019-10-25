// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public class UpdatePatternDto
    {
        /// <summary>
        /// The name of the suggestion.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The regex pattern.
        /// </summary>
        [Required]
        public string Pattern { get; set; }

        /// <summary>
        /// The regex message.
        /// </summary>
        public string Message { get; set; }

        public AddPattern ToAddCommand()
        {
            return SimpleMapper.Map(this, new AddPattern());
        }

        public UpdatePattern ToUpdateCommand(Guid id)
        {
            return SimpleMapper.Map(this, new UpdatePattern { PatternId = id });
        }
    }
}
