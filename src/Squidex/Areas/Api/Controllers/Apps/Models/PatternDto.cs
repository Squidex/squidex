// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class PatternDto : Resource
    {
        /// <summary>
        /// Unique id of the pattern.
        /// </summary>
        public Guid Id { get; set; }

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

        public static PatternDto FromPattern(Guid id, AppPattern pattern, ApiController controller, string app)
        {
            var result = SimpleMapper.Map(pattern, new PatternDto { Id = id });

            return result.CreateLinks(controller, app);
        }

        private PatternDto CreateLinks(ApiController controller, string app)
        {
            var values = new { app, id = Id };

            if (controller.HasPermission(Permissions.AppPatternsUpdate, app))
            {
                AddPutLink("update", controller.Url<AppPatternsController>(x => nameof(x.UpdatePattern), values));
            }

            if (controller.HasPermission(Permissions.AppPatternsDelete, app))
            {
                AddDeleteLink("delete", controller.Url<AppPatternsController>(x => nameof(x.DeletePattern), values));
            }

            return this;
        }
    }
}
