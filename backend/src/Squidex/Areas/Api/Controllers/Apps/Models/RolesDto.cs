﻿// ==========================================================================
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
    public sealed class RolesDto : Resource
    {
        /// <summary>
        /// The roles.
        /// </summary>
        [Required]
        public RoleDto[] Items { get; set; }

        public static RolesDto FromApp(IAppEntity app, ApiController controller)
        {
            var appName = app.Name;

            var result = new RolesDto
            {
                Items =
                    app.Roles.All
                        .Select(x => RoleDto.FromRole(x, app))
                        .Select(x => x.WithLinks(controller, appName))
                        .OrderBy(x => x.Name)
                        .ToArray()
            };

            return result.CreateLinks(controller, appName);
        }

        private RolesDto CreateLinks(ApiController controller, string app)
        {
            var values = new { app };

            AddSelfLink(controller.Url<AppRolesController>(x => nameof(x.GetRoles), values));

            if (controller.HasPermission(Permissions.AppRolesCreate, app))
            {
                AddPostLink("create", controller.Url<AppRolesController>(x => nameof(x.PostRole), values));
            }

            return this;
        }
    }
}
