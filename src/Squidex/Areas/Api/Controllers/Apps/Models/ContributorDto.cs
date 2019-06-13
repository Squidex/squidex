// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Squidex.Shared;
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
            if (!controller.IsUser(result.ContributorId))
            {
                if (controller.HasPermission(Permissions.AppContributorsAssign, app))
                {
                    result.AddPostLink("update", controller.Url<AppContributorsController>(x => nameof(x.PostContributor), new { app }));
                }

                if (controller.HasPermission(Permissions.AppContributorsRevoke, app))
                {
                    result.AddDeleteLink("delete", controller.Url<AppContributorsController>(x => nameof(x.DeleteContributor), new { app, id = result.ContributorId }));
                }
            }

            return result;
        }
    }
}
