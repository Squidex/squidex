// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.IdentityServer.Controllers.Setup
{
    public sealed class CreateUserModel
    {
        [LocalizedRequired]
        public string Email { get; set; }

        [LocalizedRequired]
        public string Password { get; set; }

        [LocalizedRequiredAttribute]
        public string PasswordConfirm { get; set; }
    }
}
