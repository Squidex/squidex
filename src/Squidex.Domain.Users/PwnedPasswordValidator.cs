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
using Squidex.Infrastructure.Log;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users
{
    public sealed class PwnedPasswordValidator : IPasswordValidator<IUser>
    {
        private const string ErrorCode = "PwnedError";
        private const string ErrorText = "This password has previously appeared in a data breach and should never be used. If you've ever used it anywhere before, change it!";
        private static readonly IdentityResult Error = IdentityResult.Failed(new IdentityError { Code = ErrorCode, Description = ErrorText });

        private readonly HaveIBeenPwnedRestClient client = new HaveIBeenPwnedRestClient();
        private readonly ISemanticLog log;

        public PwnedPasswordValidator(ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));

            this.log = log;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<IUser> manager, IUser user, string password)
        {
            try
            {
                var isBreached = await client.IsPasswordPwned(password);

                if (isBreached)
                {
                    return Error;
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
