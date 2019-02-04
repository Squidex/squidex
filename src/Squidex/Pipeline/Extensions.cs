// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Config;
using Squidex.Infrastructure.Security;

namespace Squidex.Pipeline
{
    public static class Extensions
    {
        public static bool IsFrontendClient(this ClaimsPrincipal principal)
        {
            return principal.IsInClient(Constants.FrontendClient);
        }

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
    }
}
