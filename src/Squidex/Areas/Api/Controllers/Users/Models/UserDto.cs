// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared.Users;
using Squidex.Web;
using AllPermissions = Squidex.Shared.Permissions;

namespace Squidex.Areas.Api.Controllers.Users.Models
{
    public sealed class UserDto : Resource
    {
        /// <summary>
        /// The id of the user.
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// The email of the user. Unique value.
        /// </summary>
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// The display name (usually first name and last name) of the user.
        /// </summary>
        [Required]
        public string DisplayName { get; set; }

        /// <summary>
        /// Determines if the user is locked.
        /// </summary>
        [Required]
        public bool IsLocked { get; set; }

        /// <summary>
        /// Additional permissions for the user.
        /// </summary>
        [Required]
        public IEnumerable<string> Permissions { get; set; }

        public static UserDto FromUser(IUser user, ApiController controller)
        {
            var userPermssions = user.Permissions().ToIds();
            var userName = user.DisplayName();

            var result = SimpleMapper.Map(user, new UserDto { DisplayName = userName, Permissions = userPermssions });

            return result.CreateLinks(controller);
        }

        private UserDto CreateLinks(ApiController controller)
        {
            var values = new { id = Id };

            if (controller is UserManagementController)
            {
                AddSelfLink(controller.Url<UserManagementController>(c => nameof(c.GetUser), values));
            }
            else
            {
                AddSelfLink(controller.Url<UsersController>(c => nameof(c.GetUser), values));
            }

            if (!controller.IsUser(Id))
            {
                if (controller.HasPermission(AllPermissions.AdminUsersLock) && !IsLocked)
                {
                    AddPutLink("lock", controller.Url<UserManagementController>(c => nameof(c.LockUser), values));
                }

                if (controller.HasPermission(AllPermissions.AdminUsersUnlock) && IsLocked)
                {
                    AddPutLink("unlock", controller.Url<UserManagementController>(c => nameof(c.UnlockUser), values));
                }
            }

            if (controller.HasPermission(AllPermissions.AdminUsersUpdate))
            {
                AddPutLink("update", controller.Url<UserManagementController>(c => nameof(c.PutUser), values));
            }

            AddGetLink("picture", controller.Url<UsersController>(c => nameof(c.GetUserPicture), values));

            return this;
        }
    }
}
