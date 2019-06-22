// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class PatternsDto : Resource
    {
        /// <summary>
        /// The patterns.
        /// </summary>
        [Required]
        public PatternDto[] Items { get; set; }

        public static PatternsDto FromApp(IAppEntity app, ApiController controller)
        {
            var result = new PatternsDto
            {
                Items = app.Patterns.Select(x => PatternDto.FromPattern(x.Key, x.Value, controller, app.Name)).ToArray()
            };

            return result.CreateLinks(controller, app.Name);
        }

        private PatternsDto CreateLinks(ApiController controller, string app)
        {
            var values = new { app };

            AddSelfLink(controller.Url<AppPatternsController>(x => nameof(x.GetPatterns), values));

            if (controller.HasPermission(Permissions.AppPatternsCreate, app))
            {
                AddPostLink("create", controller.Url<AppPatternsController>(x => nameof(x.PostPattern), values));
            }

            return this;
        }
    }
}
