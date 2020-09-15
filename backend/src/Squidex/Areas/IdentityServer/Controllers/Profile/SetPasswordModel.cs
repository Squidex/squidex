// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.IdentityServer.Controllers.Profile
{
    public class SetPasswordModel
    {
        [LocalizedRequired]
        public string Password { get; set; }

        [LocalizedRequiredAttribute]
        public string PasswordConfirm { get; set; }
    }
}
