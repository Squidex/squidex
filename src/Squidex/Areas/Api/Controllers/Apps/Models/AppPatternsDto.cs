// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AppPatternsDto : Resource
    {
        /// <summary>
        /// The patterns.
        /// </summary>
        [Required]
        public AppPatternDto[] Items { get; set; }

        public static AppPatternsDto FromApp(IAppEntity app, ApiController controller)
        {
            var result = new AppPatternsDto
            {
                Items = app.Patterns.Select(x => AppPatternDto.FromPattern(x.Key, x.Value, controller, app.Name)).ToArray()
            };

            return result.CreateLinks(controller, app.Name);
        }

        private AppPatternsDto CreateLinks(ApiController controller, string app)
        {
            return this;
        }
    }
}
