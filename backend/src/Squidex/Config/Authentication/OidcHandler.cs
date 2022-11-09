// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Squidex.Shared.Identity;

namespace Squidex.Config.Authentication;

public sealed class OidcHandler : OpenIdConnectEvents
{
    private readonly MyIdentityOptions options;

    public OidcHandler(MyIdentityOptions options)
    {
        this.options = options;
    }

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
}
