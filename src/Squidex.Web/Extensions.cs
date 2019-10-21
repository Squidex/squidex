// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure.Security;

namespace Squidex.Web
{
    public static class Extensions
    {
        public static string GetClientId(this ClaimsPrincipal principal)
        {
            var clientId = principal.FindFirst(OpenIdClaims.ClientId)?.Value;

            return clientId?.GetClientParts().ClientId;
        }

        public static (string App, string ClientId) GetClientParts(this string clientId)
        {
            var parts = clientId.Split(':', '~');

            if (parts.Length == 1)
            {
                return (null, parts[0]);
            }

            if (parts.Length == 2)
            {
                return (parts[0], parts[1]);
            }

            return (null, null);
        }

        public static bool IsUser(this ApiController controller, string userId)
        {
            var subject = controller.User.OpenIdSubject();

            return string.Equals(subject, userId, StringComparison.OrdinalIgnoreCase);
        }

        public static bool TryGetHeaderString(this IHeaderDictionary headers, string header, out string result)
        {
            result = null;

            if (headers.TryGetValue(header, out var value))
            {
                string valueString = value;

                if (!string.IsNullOrWhiteSpace(valueString))
                {
                    result = valueString;
                    return true;
                }
            }

            return false;
        }
    }
}
