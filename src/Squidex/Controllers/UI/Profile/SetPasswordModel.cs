// ==========================================================================
//  SetPasswordModel.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.UI.Profile
{
    public class SetPasswordModel
    {
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Passwords must be identitical.")]
        public string PasswordConfirm { get; set; }
    }
}
