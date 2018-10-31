// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;

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
        /// Associated list of permissions.
        /// </summary>
        [Required]
        public string[] Permissions { get; set; }

        public static RoleDto FromRole(Role role)
        {
            return new RoleDto { Name = role.Name, Permissions = role.Permissions.Select(x => x.Id).ToArray() };
        }
    }
}
