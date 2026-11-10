// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;

namespace Squidex.Config.Authentication;

public static class VerifiedEmailExtensions
{
    // Not prefixed with 'urn:squidex:' so that it is never stored as a custom user claim.
    private const string ClaimType = "squidex_email_verified";

    public static void SetEmailVerified(this ClaimsIdentity? identity, bool verified)
    {
        if (identity == null)
        {
            return;
        }

        // Never trust a value that has been provided by the external identity provider.
        foreach (var claim in identity.FindAll(ClaimType).ToList())
        {
            identity.RemoveClaim(claim);
        }

        if (verified)
        {
            identity.AddClaim(new Claim(ClaimType, "true"));
        }
    }

    public static bool IsEmailVerified(this ClaimsPrincipal principal)
    {
        return principal.HasClaim(ClaimType, "true");
    }
}
