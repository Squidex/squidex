// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;

namespace Squidex.Areas.IdentityServer.Controllers.Profile
{
    public class ChangePasswordModel
    {
        [LocalizedRequired]
        public string OldPassword { get; set; }

        [LocalizedRequired]
        public string Password { get; set; }

        [LocalizedCompare(nameof(Password))]
        public string PasswordConfirm { get; set; }
    }
}
