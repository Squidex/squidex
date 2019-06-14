// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Entities.Apps;
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
            var result = new RolesDto
            {
                Items = app.Roles.Values.Select(x => RoleDto.FromRole(x, app, controller)).ToArray()
            };

            return result.CreateLinks(controller, app.Name);
        }

        private RolesDto CreateLinks(ApiController controller, string app)
        {
            return this;
        }
    }
}
