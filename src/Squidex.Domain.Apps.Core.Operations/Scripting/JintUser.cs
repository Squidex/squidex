// ==========================================================================
//  JintUser.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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
        public JintUser(Engine engine, ClaimsPrincipal principal)
            : base(engine)
        {
            var subjectId = principal.OpenIdSubject();

            var isClient = string.IsNullOrWhiteSpace(subjectId);

            if (!isClient)
            {
                FastAddProperty("id", subjectId, false, true, false);
                FastAddProperty("isClient", false, false, true, false);
            }
            else
            {
                FastAddProperty("id", principal.OpenIdClientId(), false, true, false);
                FastAddProperty("isClient", true, false, true, false);
            }

            FastAddProperty("email", principal.OpenIdEmail(), false, true, false);

            var claimsInstance = new ObjectInstance(engine);

            foreach (var group in principal.Claims.GroupBy(x => x.Type))
            {
                claimsInstance.FastAddProperty(group.Key, engine.Array.Construct(group.Select(x => new JsValue(x.Value)).ToArray()), false, true, false);
            }

            FastAddProperty("claims", claimsInstance, false, true, false);
        }
    }
}
