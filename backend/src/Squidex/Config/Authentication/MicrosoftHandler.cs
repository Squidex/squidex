// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OAuth;
using Squidex.Shared.Identity;

namespace Squidex.Config.Authentication;

public sealed class MicrosoftHandler : OAuthEvents
{
    public override Task CreatingTicket(OAuthCreatingTicketContext context)
    {
        string? displayName = null;

        if (context.User.TryGetProperty("displayName", out var element1) && element1.ValueKind == JsonValueKind.String)
        {
            displayName = element1.GetString();
        }

        if (!string.IsNullOrEmpty(displayName))
        {
            context.Identity?.AddClaim(new Claim(SquidexClaimTypes.DisplayName, displayName));
        }

        string? id = null;

        if (context.User.TryGetProperty("id", out var element2) && element2.ValueKind == JsonValueKind.String)
        {
            id = element2.GetString();
        }

        if (!string.IsNullOrEmpty(id))
        {
            var pictureUrl = $"https://apis.live.net/v5.0/{id}/picture";

            context.Identity?.AddClaim(new Claim(SquidexClaimTypes.PictureUrl, pictureUrl));
        }

        UseUserPrincipalNameAsEmail(context);

        return base.CreatingTicket(context);
    }

    private static void UseUserPrincipalNameAsEmail(OAuthCreatingTicketContext context)
    {
        var identity = context.Identity;

        if (identity == null)
        {
            return;
        }

        // The 'mail' attribute can be set to any value by the admin of any tenant, therefore we cannot trust it.
        foreach (var claim in identity.FindAll(ClaimTypes.Email).ToList())
        {
            identity.RemoveClaim(claim);
        }

        var upn = context.User.Deserialize<MicrosoftUser>(JsonSerializerOptions.Web)?.UserPrincipalName;

        // The UPN must use a domain that has been verified by the tenant. Guest accounts contain '#EXT#' and are not verified.
        var isValid = !string.IsNullOrWhiteSpace(upn) && upn.Contains('@', StringComparison.Ordinal) && !upn.Contains('#', StringComparison.Ordinal);

        if (isValid)
        {
            identity.AddClaim(new Claim(ClaimTypes.Email, upn!));
        }

        identity.SetEmailVerified(isValid);
    }

    private sealed class MicrosoftUser
    {
        public string? UserPrincipalName { get; init; }
    }
}
