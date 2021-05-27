// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class PatternDto
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
        public string Regex { get; set; }

        /// <summary>
        /// The regex message.
        /// </summary>
        public string? Message { get; set; }

        public static PatternDto FromPattern(Pattern pattern)
        {
            return SimpleMapper.Map(pattern, new PatternDto());
        }

        public Pattern ToPattern()
        {
            return new Pattern(Name, Regex)
            {
                Message = Message
            };
        }
    }
}
