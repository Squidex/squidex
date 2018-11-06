// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Entities.Apps;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class RolesDto
    {
        /// <summary>
        /// The app roles.
        /// </summary>
        [Required]
        public RoleDto[] Roles { get; set; }

        public static RolesDto FromApp(IAppEntity app)
        {
            var roles = app.Roles.Values.Select(x => RoleDto.FromRole(x, app)).ToArray();

            return new RolesDto { Roles = roles };
        }
    }
}
