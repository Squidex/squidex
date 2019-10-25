﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Security.Claims;

namespace Squidex.Infrastructure.Security
{
    public static class Extensions
    {
        public static RefToken? Token(this ClaimsPrincipal principal)
        {
            var subjectId = principal.OpenIdSubject();

            if (!string.IsNullOrWhiteSpace(subjectId))
            {
                return new RefToken(RefTokenType.Subject, subjectId);
            }

            var clientId = principal.OpenIdClientId();

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                return new RefToken(RefTokenType.Client, clientId);
            }

            return null;
        }

        public static string? OpenIdSubject(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == OpenIdClaims.Subject)?.Value;
        }

        public static string? OpenIdClientId(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == OpenIdClaims.ClientId)?.Value;
        }

        public static string? UserOrClientId(this ClaimsPrincipal principal)
        {
            return principal.OpenIdSubject() ?? principal.OpenIdClientId();
        }

        public static string? OpenIdPreferredUserName(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == OpenIdClaims.PreferredUserName)?.Value;
        }

        public static string? OpenIdName(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == OpenIdClaims.Name)?.Value;
        }

        public static string? OpenIdNickName(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == OpenIdClaims.NickName)?.Value;
        }

        public static string? OpenIdEmail(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(x => x.Type == OpenIdClaims.Email)?.Value;
        }

        public static bool IsInClient(this ClaimsPrincipal principal, string client)
        {
            return principal.Claims.Any(x => x.Type == OpenIdClaims.ClientId && string.Equals(x.Value, client, StringComparison.OrdinalIgnoreCase));
        }
    }
}
