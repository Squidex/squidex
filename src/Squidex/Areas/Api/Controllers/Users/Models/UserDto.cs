// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared.Users;

namespace Squidex.Areas.Api.Controllers.Users.Models
{
    public sealed class UserDto
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

        public static UserDto FromUser(IUser user)
        {
            return SimpleMapper.Map(user, new UserDto { DisplayName = user.DisplayName() });
        }
    }
}
