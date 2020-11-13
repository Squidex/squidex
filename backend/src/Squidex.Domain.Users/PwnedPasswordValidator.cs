// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SharpPwned.NET;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Log;

namespace Squidex.Domain.Users
{
    public sealed class PwnedPasswordValidator : IPasswordValidator<IdentityUser>
    {
        private readonly HaveIBeenPwnedRestClient client = new HaveIBeenPwnedRestClient();
        private readonly ISemanticLog log;

        public PwnedPasswordValidator(ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));

            this.log = log;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<IdentityUser> manager, IdentityUser user, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return IdentityResult.Success;
            }

            try
            {
                var isBreached = await client.IsPasswordPwned(password);

                if (isBreached)
                {
                    var errorText = T.Get("security.passwordStolen");

                    return IdentityResult.Failed(new IdentityError { Code = "PwnedError", Description = errorText });
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("operation", "CheckPasswordPwned")
                    .WriteProperty("status", "Failed"));
            }

            return IdentityResult.Success;
        }
    }
}
