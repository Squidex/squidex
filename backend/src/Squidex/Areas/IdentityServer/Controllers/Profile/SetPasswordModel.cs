// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;

namespace Squidex.Areas.IdentityServer.Controllers.Profile
{
    public class SetPasswordModel
    {
        [LocalizedRequired]
        public string Password { get; set; }

        [LocalizedRequired]
        public string PasswordConfirm { get; set; }
    }
}
