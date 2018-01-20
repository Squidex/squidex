// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.Api.Controllers.Users.Models
{
    public sealed class CreateUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
