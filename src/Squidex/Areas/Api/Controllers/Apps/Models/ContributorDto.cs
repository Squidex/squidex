// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Squidex.Shared;
using Squidex.Shared.Users;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class ContributorDto : Resource
    {
        private const string NotFound = "- not found -";

        /// <summary>
        /// The id of the user that contributes to the app.
        /// </summary>
        [Required]
        public string ContributorId { get; set; }

        /// <summary>
        /// The display name.
        /// </summary>
        [Required]
        public string ContributorName { get; set; }

        /// <summary>
        /// The role of the contributor.
        /// </summary>
        public string Role { get; set; }

        public static ContributorDto FromIdAndRole(string id, string role)
        {
            var result = new ContributorDto { ContributorId = id, Role = role };

            return result;
        }

        public ContributorDto WithUser(IDictionary<string, IUser> users)
        {
            if (users.TryGetValue(ContributorId, out var user))
            {
                ContributorName = user.DisplayName();
            }
            else
            {
                ContributorName = NotFound;
            }

            return this;
        }

        public ContributorDto WithLinks(ApiController controller, string app)
        {
            if (!controller.IsUser(ContributorId))
            {
                if (controller.HasPermission(Permissions.AppContributorsAssign, app))
                {
                    AddPostLink("update", controller.Url<AppContributorsController>(x => nameof(x.PostContributor), new { app }));
                }

                if (controller.HasPermission(Permissions.AppContributorsRevoke, app))
                {
                    AddDeleteLink("delete", controller.Url<AppContributorsController>(x => nameof(x.DeleteContributor), new { app, id = ContributorId }));
                }
            }

            return this;
        }
    }
}
