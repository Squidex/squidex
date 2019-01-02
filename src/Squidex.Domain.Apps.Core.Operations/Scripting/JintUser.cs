// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Security.Claims;
using Jint;
using Jint.Runtime.Interop;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public static class JintUser
    {
        private static readonly char[] ClaimSeparators = { '/', '.', ':' };

        public static ObjectWrapper Create(Engine engine, ClaimsPrincipal principal)
        {
            var id = principal.OpenIdSubject();

            var isClient = string.IsNullOrWhiteSpace(id);

            if (isClient)
            {
                id = principal.OpenIdClientId();
            }

            var claims =
                principal.Claims.GroupBy(x => x.Type)
                    .ToDictionary(
                        x => x.Key.Split(ClaimSeparators).Last(),
                        x => x.Select(y => y.Value).ToArray());

            return new ObjectWrapper(engine, new
            {
                Id = id,
                IsClient = isClient,
                Email = principal.OpenIdEmail(),
                Claims = claims,
            });
        }
    }
}
