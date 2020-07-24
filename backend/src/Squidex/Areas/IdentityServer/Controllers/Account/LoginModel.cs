// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;

namespace Squidex.Areas.IdentityServer.Controllers.Account
{
    public sealed class LoginModel
    {
        [LocalizedRequired]
        public string Email { get; set; }

        [LocalizedRequired]
        public string Password { get; set; }
    }
}
