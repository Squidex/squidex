// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class RoleDto
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
        /// Associated list of permissions.
        /// </summary>
        [Required]
        public IEnumerable<string> Permissions { get; set; }

        public static RoleDto FromRole(Role role, IAppEntity app)
        {
            var permissions = role.Permissions.WithoutApp(app.Name);

            return new RoleDto
            {
                Name = role.Name,
                NumClients = app.Clients.Count(x => string.Equals(x.Value.Role, role.Name, StringComparison.OrdinalIgnoreCase)),
                NumContributors = app.Contributors.Count(x => string.Equals(x.Value, role.Name, StringComparison.OrdinalIgnoreCase)),
                Permissions = permissions.ToIds()
            };
        }
    }
}
