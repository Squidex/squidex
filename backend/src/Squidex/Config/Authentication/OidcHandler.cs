// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Squidex.Infrastructure.Security;
using Squidex.Shared.Identity;

namespace Squidex.Config.Authentication;

public sealed class OidcHandler(MyIdentityOptions options, bool trustEmails = true, string? emailDomain = null) : OpenIdConnectEvents
{
    public override Task TokenValidated(TokenValidatedContext context)
    {
        var identity = (ClaimsIdentity)context.Principal!.Identity!;

        if (!string.IsNullOrWhiteSpace(options.OidcRoleClaimType) && options.OidcRoleMapping?.Count >= 0)
        {
            var permissions = options.OidcRoleMapping
                .Where(r => identity.HasClaim(options.OidcRoleClaimType, r.Key))
                .Select(r => r.Value)
                .SelectMany(r => r)
                .Distinct();

            foreach (var permission in permissions)
            {
                identity.AddClaim(new Claim(SquidexClaimTypes.Permissions, permission));
            }
        }

        return base.TokenValidated(context);
    }

    public override Task TicketReceived(TicketReceivedContext context)
    {
        var principal = context.Principal;

        if (principal != null)
        {
            (principal.Identity as ClaimsIdentity).SetEmailVerified(IsEmailVerified(principal));
        }

        return base.TicketReceived(context);
    }

    public override Task RedirectToIdentityProviderForSignOut(RedirectContext context)
    {
        if (!string.IsNullOrEmpty(options.OidcOnSignoutRedirectUrl))
        {
            var logoutUri = options.OidcOnSignoutRedirectUrl;

            context.Response.Redirect(logoutUri);
            context.HandleResponse();

            return Task.CompletedTask;
        }

        return base.RedirectToIdentityProviderForSignOut(context);
    }

    private bool IsEmailVerified(ClaimsPrincipal principal)
    {
        if (!trustEmails)
        {
            return false;
        }

        var email = principal.GetEmail();

        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        // Dynamic providers can only vouch for emails of their own domain.
        if (emailDomain != null && !email.EndsWith($"@{emailDomain}", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var verified = principal.FindFirst("email_verified")?.Value;

        if (verified != null)
        {
            return bool.TryParse(verified, out var isVerified) && isVerified;
        }

        return !options.OidcRequireVerifiedEmail;
    }
}
