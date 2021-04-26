// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class PatternDto
    {
        /// <summary>
        /// Unique id of the pattern.
        /// </summary>
        public DomainId Id { get; set; }

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
    }
}
