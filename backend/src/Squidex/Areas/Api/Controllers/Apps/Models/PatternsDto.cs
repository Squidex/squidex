// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class PatternsDto : Resource
    {
        /// <summary>
        /// The patterns.
        /// </summary>
        [LocalizedRequired]
        public PatternDto[] Items { get; set; }

        public static PatternsDto FromApp(IAppEntity app, Resources resources)
        {
            var result = new PatternsDto
            {
                Items = app.Patterns.Select(x => PatternDto.FromPattern(x.Key, x.Value, resources)).ToArray()
            };

            return result.CreateLinks(resources);
        }

        private PatternsDto CreateLinks(Resources resources)
        {
            var values = new { app = resources.App };

            AddSelfLink(resources.Url<AppPatternsController>(x => nameof(x.GetPatterns), values));

            if (resources.CanCreatePattern)
            {
                AddPostLink("create", resources.Url<AppPatternsController>(x => nameof(x.PostPattern), values));
            }

            return this;
        }
    }
}
