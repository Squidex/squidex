// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Entities.Apps;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class RolesDto
    {
        /// <summary>
        /// The roles.
        /// </summary>
        [Required]
        public RoleDto[] Roles { get; set; }

        /// <summary>
        /// Suggested permissions.
        /// </summary>
        public string[] AllPermissions { get; set; }

        public static RolesDto FromApp(IAppEntity app, List<string> permissions)
        {
            var roles = app.Roles.Values.Select(RoleDto.FromRole).ToArray();

            return new RolesDto { Roles = roles, AllPermissions = permissions.ToList() };
        }
    }
}
