// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Web.Pipeline;

public sealed class AppResolver : IAsyncActionFilter
{
    private readonly IAppProvider appProvider;

    public AppResolver(IAppProvider appProvider)
    {
        this.appProvider = appProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (context.RouteData.Values.TryGetValue("app", out var appValue))
        {
            var appName = appValue?.ToString();

            if (string.IsNullOrWhiteSpace(appName))
            {
                context.Result = new NotFoundResult();
                return;
            }

            var isFrontend = user.IsInClient(DefaultClients.Frontend);

            var app = await appProvider.GetAppAsync(appName, !isFrontend, default);

            if (app == null)
            {
                var log = context.HttpContext.RequestServices?.GetService<ILogger<AppResolver>>();

                log?.LogWarning("Cannot find app with the given name {name}.", appName);

                context.Result = new NotFoundResult();
                return;
            }

            string? clientId = null;

            var (role, permissions) = FindByOpenIdSubject(app, user, isFrontend);

            if (permissions == null)
            {
                (clientId, role, permissions) = FindByOpenIdClient(app, user, isFrontend);
            }

            if (permissions == null)
            {
                (clientId, role, permissions) = FindAnonymousClient(app, isFrontend);
            }

            if (permissions != null)
            {
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

            var requestContext = new Context(context.HttpContext.User, app).WithHeaders(context.HttpContext);

            if (!AllowAnonymous(context) && !HasPermission(appName, requestContext))
            {
                if (string.IsNullOrWhiteSpace(user.Identity?.AuthenticationType))
                {
                    context.Result = new UnauthorizedResult();
                }
                else
                {
                    var log = context.HttpContext.RequestServices?.GetService<ILogger<AppResolver>>();

                    log?.LogWarning("Authenticated user has no permission to access the app {name} with ID {id}.",
                        app.Id,
                        app.Name);

                    context.Result = new NotFoundResult();
                }

                return;
            }

            context.HttpContext.Features.Set(requestContext);
            context.HttpContext.Features.Set<IAppFeature>(new AppFeature(app));
            context.HttpContext.Response.Headers.Add("X-AppId", app.Id.ToString());
        }

        await next();
    }

    private static bool HasPermission(string appName, Context requestContext)
    {
        return requestContext.UserPermissions.Includes(PermissionIds.ForApp(PermissionIds.App, appName));
    }

    private static bool AllowAnonymous(ActionExecutingContext context)
    {
        return context.ActionDescriptor.EndpointMetadata.Any(x => x is AllowAnonymousAttribute);
    }

    private static (string?, string?, PermissionSet?) FindByOpenIdClient(IAppEntity app, ClaimsPrincipal user, bool isFrontend)
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

    private static (string?, string?, PermissionSet?) FindAnonymousClient(IAppEntity app, bool isFrontend)
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

    private static (string?, PermissionSet?) FindByOpenIdSubject(IAppEntity app, ClaimsPrincipal user, bool isFrontend)
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
