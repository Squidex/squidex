// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Users;

namespace Squidex.Areas.IdentityServer.Controllers.Profile
{
    public class ChangeProfileModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email is not valid.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "DisplayName is required.")]
        public string DisplayName { get; set; }

        public bool IsHidden { get; set; }

        public UserValues ToValues()
        {
            return new UserValues { Email = Email, DisplayName = DisplayName, Hidden = IsHidden };
        }
    }
}
