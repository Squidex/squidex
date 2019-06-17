// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Users;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Users.Models
{
    public sealed class UsersDto : Resource
    {
        /// <summary>
        /// The total number of users.
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// The users.
        /// </summary>
        [Required]
        public UserDto[] Items { get; set; }

        public static UsersDto FromResults(IEnumerable<UserWithClaims> items, long total, ApiController controller)
        {
            var result = new UsersDto
            {
                Total = total,
                Items = items.Select(x => UserDto.FromUser(x, controller)).ToArray()
            };

            return result.CreateLinks(controller);
        }

        private UsersDto CreateLinks(ApiController controller)
        {
            AddSelfLink(controller.Url<UserManagementController>(c => nameof(c.GetUsers)));

            if (controller.HasPermission(Permissions.AdminUsersCreate))
            {
                AddPostLink("create", controller.Url<UserManagementController>(c => nameof(c.PostUser)));
            }

            return this;
        }
    }
}
