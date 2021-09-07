// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Jint;
using Jint.Runtime.Interop;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public static class JintUser
    {
        private static readonly char[] ClaimSeparators = { '/', '.', ':' };

        public static ObjectWrapper Create(Engine engine, IUser user)
        {
            var isClient = user.Claims.Any(x => x.Type == OpenIdClaims.ClientId);

            return CreateUser(
                engine,
                user.Id,
                isClient,
                user.Email,
                user.Claims.DisplayName(),
                user.Claims);
        }

        public static ObjectWrapper Create(Engine engine, ClaimsPrincipal principal)
        {
            var token = principal.Token()!;

            return CreateUser(
                engine,
                token.Identifier,
                token.Type == RefTokenType.Client,
                principal.OpenIdEmail()!,
                principal.Claims.DisplayName(),
                principal.Claims);
        }

        private static ObjectWrapper CreateUser(Engine engine, string id, bool isClient, string email, string? name, IEnumerable<Claim> allClaims)
        {
            var claims =
                allClaims.GroupBy(x => x.Type.Split(ClaimSeparators).Last())
                    .ToDictionary(
                        x => x.Key,
                        x => x.Select(y => y.Value).ToArray());

            return new ObjectWrapper(engine, new
            {
                id,
                isClient,
                isUser = !isClient,
                email,
                name,
                claims
            });
        }
    }
}
