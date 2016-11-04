// ==========================================================================
//  Extensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Security.Claims;

namespace Squidex.Infrastructure.Security
{
    public static class Extensions
    {
        public static string OpenIdSubject(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == OpenIdClaims.Subject)?.Value;
        }

        public static string OpenIdPreferredUserName(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == OpenIdClaims.PreferredUserName)?.Value;
        }

        public static string OpenIdName(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == OpenIdClaims.Name)?.Value;
        }

        public static string OpenIdNickName(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == OpenIdClaims.NickName)?.Value;
        }

        public static string OpenIdEmail(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == OpenIdClaims.Email)?.Value;
        }
    }
}
