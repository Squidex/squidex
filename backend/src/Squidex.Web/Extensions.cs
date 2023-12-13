// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Infrastructure.Security;

namespace Squidex.Web;

public static class Extensions
{
    public static string? GetClientId(this ClaimsPrincipal principal)
    {
        var clientId = principal.FindFirst(OpenIdClaims.ClientId)?.Value;

        return clientId?.GetClientParts().ClientId;
    }

    public static (string? App, string? ClientId) GetClient(this ClaimsPrincipal principal)
    {
        var clientId = principal.FindFirst(OpenIdClaims.ClientId)?.Value;

        return clientId.GetClientParts();
    }

    public static (string? App, string? ClientId) GetClientParts(this string? clientId)
    {
        if (clientId == null)
        {
            return (null, null);
        }

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
}
