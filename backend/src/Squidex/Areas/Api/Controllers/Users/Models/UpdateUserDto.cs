﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Users;
using Squidex.Infrastructure.Security;

namespace Squidex.Areas.Api.Controllers.Users.Models
{
    public sealed class UpdateUserDto
    {
        /// <summary>
        /// The email of the user. Unique value.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// The display name (usually first name and last name) of the user.
        /// </summary>
        [Required]
        public string DisplayName { get; set; }

        /// <summary>
        /// The password of the user.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Additional permissions for the user.
        /// </summary>
        [Required]
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
