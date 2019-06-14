// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Web;
using AllPermissions = Squidex.Shared.Permissions;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class RoleDto : Resource
    {
        /// <summary>
        /// The role name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The number of clients with this role.
        /// </summary>
        public int NumClients { get; set; }

        /// <summary>
        /// The number of contributors with this role.
        /// </summary>
        public int NumContributors { get; set; }

        /// <summary>
        /// Indicates if the role is an builtin default role.
        /// </summary>
        public bool IsDefaultRole { get; set; }

        /// <summary>
        /// Associated list of permissions.
        /// </summary>
        [Required]
        public IEnumerable<string> Permissions { get; set; }

        public static RoleDto FromRole(Role role, IAppEntity app, ApiController controller)
        {
            var permissions = role.Permissions.WithoutApp(app.Name);

            var result = new RoleDto
            {
                Name = role.Name,
                NumClients = app.Clients.Count(x => Role.IsRole(x.Value.Role, role.Name)),
                NumContributors = app.Contributors.Count(x => Role.IsRole(x.Value, role.Name)),
                Permissions = permissions.ToIds(),
                IsDefaultRole = Role.IsDefaultRole(role.Name)
            };

            return result.CreateLinks(controller, app.Name);
        }

        private RoleDto CreateLinks(ApiController controller, string app)
        {
            var values = new { app, name = Name };

            if (!IsDefaultRole)
            {
                if (controller.HasPermission(AllPermissions.AppRolesUpdate, app) && NumClients == 0 && NumContributors == 0)
                {
                    AddPutLink("update", controller.Url<AppRolesController>(x => nameof(x.UpdateRole), values));
                }

                if (controller.HasPermission(AllPermissions.AppRolesDelete, app))
                {
                    AddDeleteLink("delete", controller.Url<AppRolesController>(x => nameof(x.DeleteRole), values));
                }
            }

            return this;
        }
    }
}
