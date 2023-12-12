// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Entities.TestHelpers;

public static class Mocks
{
    public static ClaimsPrincipal ApiUser(string? role = null, params string[] permissions)
    {
        return CreateUser(false, role, permissions);
    }

    public static ClaimsPrincipal ApiUser(string? role = null, string? permission = null)
    {
        return CreateUser(false, role, permission);
    }

    public static ClaimsPrincipal FrontendUser(string? role = null, params string[] permissions)
    {
        return CreateUser(true, role, permissions);
    }

    public static ClaimsPrincipal FrontendUser(string? role = null, string? permission = null)
    {
        return CreateUser(true, role, permission);
    }

    public static ClaimsPrincipal CreateUser(bool isFrontend, string? role, params string?[] permissions)
    {
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        if (isFrontend)
        {
            claimsIdentity.AddClaim(new Claim(OpenIdClaims.ClientId, DefaultClients.Frontend));
        }

        if (role != null)
        {
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            if (permission != null)
            {
                claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, permission));
            }
        }

        return claimsPrincipal;
    }
}
