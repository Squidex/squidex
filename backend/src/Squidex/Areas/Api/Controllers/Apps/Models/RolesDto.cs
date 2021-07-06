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
    public sealed class RolesDto : Resource
    {
        /// <summary>
        /// The roles.
        /// </summary>
        [LocalizedRequired]
        public RoleDto[] Items { get; set; }

        public static RolesDto FromApp(IAppEntity app, Resources resources)
        {
            var result = new RolesDto
            {
                Items =
                    app.Roles.All
                        .Select(x => RoleDto.FromRole(x, app))
                        .Select(x => x.CreateLinks(resources))
                        .OrderBy(x => x.Name)
                        .ToArray()
            };

            return result.CreateLinks(resources);
        }

        private RolesDto CreateLinks(Resources resources)
        {
            var values = new { app = resources.App };

            AddSelfLink(resources.Url<AppRolesController>(x => nameof(x.GetRoles), values));

            if (resources.CanCreateRole)
            {
                AddPostLink("create", resources.Url<AppRolesController>(x => nameof(x.PostRole), values));
            }

            return this;
        }
    }
}
