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
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AppPatternDto : Resource
    {
        /// <summary>
        /// Unique id of the pattern.
        /// </summary>
        public Guid PatternId { get; set; }

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

        public static AppPatternDto FromPattern(Guid id, AppPattern pattern, ApiController controller, string app)
        {
            var result = SimpleMapper.Map(pattern, new AppPatternDto { PatternId = id });

            return result.CreateLinks(controller, app);
        }

        private AppPatternDto CreateLinks(ApiController controller, string app)
        {
            return this;
        }
    }
}
