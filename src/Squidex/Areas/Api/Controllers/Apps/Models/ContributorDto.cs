// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class ContributorDto : Resource
    {
        /// <summary>
        /// The id of the user that contributes to the app.
        /// </summary>
        [Required]
        public string ContributorId { get; set; }

        /// <summary>
        /// The role of the contributor.
        /// </summary>
        public string Role { get; set; }

        public static ContributorDto FromIdAndRole(string id, string role, ApiController controller, string app)
        {
            var result = new ContributorDto { ContributorId = id, Role = role };

            return CreateLinks(result, controller, app);
        }

        private static ContributorDto CreateLinks(ContributorDto result, ApiController controller, string app)
        {
            return result;
        }
    }
}
