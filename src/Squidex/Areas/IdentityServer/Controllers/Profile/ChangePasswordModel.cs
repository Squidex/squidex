// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.IdentityServer.Controllers.Profile
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Old Password is required.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Passwords must be identitical.")]
        public string PasswordConfirm { get; set; }
    }
}
