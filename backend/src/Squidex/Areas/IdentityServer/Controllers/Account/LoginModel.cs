// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.IdentityServer.Controllers.Account
{
    public sealed class LoginModel
    {
        [LocalizedRequired]
        public string Email { get; set; }

        [LocalizedRequiredAttribute]
        public string Password { get; set; }
    }
}
