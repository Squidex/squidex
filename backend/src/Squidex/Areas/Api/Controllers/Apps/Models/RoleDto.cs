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

        public static RoleDto FromRole(Role role, IAppEntity app)
        {
            var permissions = role.Permissions;

            var result = new RoleDto
            {
                Name = role.Name,
                NumClients = GetNumClients(role, app),
                NumContributors = GetNumContributors(role, app),
                Permissions = permissions.ToIds(),
                IsDefaultRole = role.IsDefault
            };

            return result;
        }

        private static int GetNumContributors(Role role, IAppEntity app)
        {
            return app.Contributors.Count(x => role.Equals(x.Value));
        }

        private static int GetNumClients(Role role, IAppEntity app)
        {
            return app.Clients.Count(x => role.Equals(x.Value.Role));
        }

        public RoleDto WithLinks(ApiController controller, string app)
        {
            var values = new { app, name = Name };

            if (!IsDefaultRole)
            {
                if (controller.HasPermission(AllPermissions.AppRolesUpdate, app) && NumClients == 0 && NumContributors == 0)
                {
                    AddPutLink("update", controller.Url<AppRolesController>(x => nameof(x.PutRole), values));
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
