// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public class UpdatePatternDto
    {
        /// <summary>
        /// The name of the suggestion.
        /// </summary>
        [LocalizedRequired]
        public string Name { get; set; }

        /// <summary>
        /// The regex pattern.
        /// </summary>
        [LocalizedRequired]
        public string Pattern { get; set; }

        /// <summary>
        /// The regex message.
        /// </summary>
        public string? Message { get; set; }

        public AddPattern ToAddCommand()
        {
            return SimpleMapper.Map(this, new AddPattern());
        }

        public UpdatePattern ToUpdateCommand(DomainId id)
        {
            return SimpleMapper.Map(this, new UpdatePattern { PatternId = id });
        }
    }
}
