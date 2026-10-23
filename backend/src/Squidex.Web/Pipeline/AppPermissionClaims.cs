// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Web.Pipeline;

public static class AppPermissionClaims
{
    public static void Enrich(App app, ClaimsPrincipal user)
    {
        var isFrontend = user.IsInClient(DefaultClients.Frontend);

        string? clientId = null;

        var (role, permissions) = FindByOpenIdSubject(app, user, isFrontend);
        if (permissions == null)
        {
            (clientId, role, permissions) = FindByOpenIdClient(app, user, isFrontend);
        }

        if (permissions == null)
        {
            (role, permissions) = FindByUserInfo(app, user, isFrontend);
        }

        if (permissions == null)
        {
            (clientId, role, permissions) = FindAnonymousClient(app, isFrontend);
        }

        if (permissions == null)
        {
            return;
        }

        var identity = user.Identities.First();

        if (!string.IsNullOrWhiteSpace(role))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            identity.AddClaim(new Claim(SquidexClaimTypes.Permissions, permission.Id));
        }

        if (user.Token() == null && clientId != null)
        {
            identity.AddClaim(new Claim(OpenIdClaims.ClientId, clientId));
        }
    }

    public static bool HasAppPermission(string appName, Context requestContext)
    {
        return requestContext.UserPermissions.Includes(PermissionIds.ForApp(PermissionIds.App, appName));
    }

    private static (string?, string?, PermissionSet?) FindByOpenIdClient(App app, ClaimsPrincipal user, bool isFrontend)
    {
        var (appName, clientId) = user.GetClient();
        if (app.Name != appName || clientId == null)
        {
            return default;
        }

        if (app.TryGetClientRole(clientId, isFrontend, out var role))
        {
            return (clientId, role.Name, role.Permissions);
        }

        return default;
    }

    private static (string?, PermissionSet?) FindByUserInfo(App app, ClaimsPrincipal user, bool isFrontend)
    {
        var (_, appName, roleName) = user.GetUserInfo();
        if (app.Name != appName || roleName == null)
        {
            return default;
        }

        if (app.TryGetRole(roleName, isFrontend, out var role))
        {
            return (role.Name, role.Permissions);
        }

        return default;
    }

    private static (string?, string?, PermissionSet?) FindAnonymousClient(App app, bool isFrontend)
    {
        var client = app.Clients.FirstOrDefault(x => x.Value.AllowAnonymous);
        if (client.Value == null)
        {
            return default;
        }

        if (app.TryGetRole(client.Value.Role, isFrontend, out var role))
        {
            return (client.Key, role.Name, role.Permissions);
        }

        return default;
    }

    private static (string?, PermissionSet?) FindByOpenIdSubject(App app, ClaimsPrincipal user, bool isFrontend)
    {
        var subjectId = user.OpenIdSubject();
        if (subjectId == null)
        {
            return default;
        }

        if (app.TryGetContributorRole(subjectId, isFrontend, out var role))
        {
            return (role.Name, role.Permissions);
        }

        return default;
    }
}
