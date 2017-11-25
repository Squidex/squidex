// ==========================================================================
//  ChangeProfileModel.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.IdentityServer.Controllers.Profile
{
    public class ChangeProfileModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email is not valid.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "DisplayName is required.")]
        public string DisplayName { get; set; }
    }
}
