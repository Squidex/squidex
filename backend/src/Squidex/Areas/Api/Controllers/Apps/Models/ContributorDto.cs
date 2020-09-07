// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Shared.Users;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class ContributorDto : Resource
    {
        /// <summary>
        /// The id of the user that contributes to the app.
        /// </summary>
        [LocalizedRequired]
        public string ContributorId { get; set; }

        /// <summary>
        /// The display name.
        /// </summary>
        [LocalizedRequired]
        public string ContributorName { get; set; }

        /// <summary>
        /// The email address.
        /// </summary>
        [LocalizedRequired]
        public string ContributorEmail { get; set; }

        /// <summary>
        /// The role of the contributor.
        /// </summary>
        public string? Role { get; set; }

        public static ContributorDto FromIdAndRole(string id, string role)
        {
            var result = new ContributorDto { ContributorId = id, Role = role };

            return result;
        }

        public ContributorDto WithUser(IDictionary<string, IUser> users)
        {
            if (users.TryGetValue(ContributorId, out var user))
            {
                ContributorName = user.DisplayName()!;
                ContributorEmail = user.Email;
            }
            else
            {
                ContributorName = T.Get("common.notFoundValue");
            }

            return this;
        }

        public ContributorDto WithLinks(Resources resources)
        {
            if (!resources.IsUser(ContributorId))
            {
                var app = resources.App;

                if (resources.CanAssignContributor)
                {
                    AddPostLink("update", resources.Url<AppContributorsController>(x => nameof(x.PostContributor), new { app }));
                }

                if (resources.CanRevokeContributor)
                {
                    AddDeleteLink("delete", resources.Url<AppContributorsController>(x => nameof(x.DeleteContributor), new { app, id = ContributorId }));
                }
            }

            return this;
        }
    }
}
