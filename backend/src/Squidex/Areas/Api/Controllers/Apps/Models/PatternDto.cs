// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class PatternDto : Resource
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
        public string Pattern { get; set; }

        /// <summary>
        /// The regex message.
        /// </summary>
        public string? Message { get; set; }

        public static PatternDto FromPattern(DomainId id, AppPattern pattern, Resources resources)
        {
            var result = SimpleMapper.Map(pattern, new PatternDto { Id = id });

            return result.CreateLinks(resources);
        }

        private PatternDto CreateLinks(Resources resources)
        {
            var values = new { app = resources.App, id = Id };

            if (resources.CanUpdatePattern)
            {
                AddPutLink("update", resources.Url<AppPatternsController>(x => nameof(x.PutPattern), values));
            }

            if (resources.CanDeletePattern)
            {
                AddDeleteLink("delete", resources.Url<AppPatternsController>(x => nameof(x.DeletePattern), values));
            }

            return this;
        }
    }
}
