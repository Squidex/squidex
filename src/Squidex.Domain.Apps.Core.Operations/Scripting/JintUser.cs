// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Security.Claims;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class JintUser : ObjectInstance
    {
        private static readonly char[] ClaimSeparators = { '/', '.', ':' };

        public JintUser(Engine engine, ClaimsPrincipal principal)
            : base(engine)
        {
            var id = principal.OpenIdSubject();

            var isClient = string.IsNullOrWhiteSpace(id);

            if (isClient)
            {
                id = principal.OpenIdClientId();
            }

            FastAddProperty("id", id, false, true, false);
            FastAddProperty("isClient", isClient, false, true, false);

            FastAddProperty("email", principal.OpenIdEmail(), false, true, false);

            var claimsInstance = new ObjectInstance(engine);

            foreach (var group in principal.Claims.GroupBy(x => x.Type))
            {
                var propertyName = group.Key.Split(ClaimSeparators).Last();
                var propertyValue = engine.Array.Construct(group.Select(x => new JsValue(x.Value)).ToArray());

                claimsInstance.FastAddProperty(propertyName, propertyValue, false, true, false);
            }

            FastAddProperty("claims", claimsInstance, false, true, false);
        }
    }
}
