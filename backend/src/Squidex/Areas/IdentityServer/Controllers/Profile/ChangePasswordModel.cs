// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.IdentityServer.Controllers.Profile
{
    public class ChangePasswordModel
    {
        [LocalizedRequired]
        public string OldPassword { get; set; }

        [LocalizedRequiredAttribute]
        public string Password { get; set; }

        [LocalizedCompare(nameof(Password))]
        public string PasswordConfirm { get; set; }
    }
}
