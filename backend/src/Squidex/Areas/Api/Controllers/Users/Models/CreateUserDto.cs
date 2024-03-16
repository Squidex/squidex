// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Users;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Users.Models
{
    public sealed class CreateUserDto
    {
        /// <summary>
        /// The email of the user. Unique value.
        /// </summary>
        [LocalizedRequired]
        [LocalizedEmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// The display name (usually first name and last name) of the user.
        /// </summary>
        [LocalizedRequired]
        public string DisplayName { get; set; }

        /// <summary>
        /// The password of the user.
        /// </summary>
        [LocalizedRequired]
        public string Password { get; set; }

        /// <summary>
        /// Additional permissions for the user.
        /// </summary>
        [LocalizedRequired]
        public string[] Permissions { get; set; }

        public UserValues ToValues()
        {
            return new UserValues
            {
                Email = Email,
                DisplayName = DisplayName,
                Password = Password,
                Permissions = new PermissionSet(Permissions)
            };
        }
    }
}
