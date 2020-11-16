// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Web.Pipeline
{
    public sealed class AppResolver : IAsyncActionFilter
    {
        private readonly IAppProvider appProvider;

        public AppResolver(IAppProvider appProvider)
        {
            Guard.NotNull(appProvider, nameof(appProvider));

            this.appProvider = appProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            var appName = context.RouteData.Values["app"]?.ToString();

            if (!string.IsNullOrWhiteSpace(appName))
            {
                var isFrontend = user.IsInClient(DefaultClients.Frontend);

                var app = await appProvider.GetAppAsync(appName, !isFrontend);

                if (app == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                var (role, permissions) = FindByOpenIdSubject(app, user, isFrontend);

                if (permissions == null)
                {
                    (role, permissions) = FindByOpenIdClient(app, user, isFrontend);
                }

                if (permissions == null)
                {
                    (role, permissions) = FindAnonymousClient(app, isFrontend);
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
                }

                var requestContext = SetContext(context.HttpContext, app);

                if (!AllowAnonymous(context) && !HasPermission(appName, requestContext))
                {
                    if (string.IsNullOrWhiteSpace(user.Identity?.AuthenticationType))
                    {
                        context.Result = new UnauthorizedResult();
                    }
                    else
                    {
                        context.Result = new NotFoundResult();
                    }

                    return;
                }

                context.HttpContext.Features.Set<IAppFeature>(new AppFeature(app.NamedId()));
                context.HttpContext.Response.Headers.Add("X-AppId", app.Id.ToString());
            }
            else
            {
                SetContext(context.HttpContext, null!);
            }

            await next();
        }

        private static Context SetContext(HttpContext httpContext, IAppEntity app)
        {
            var requestContext = new Context(httpContext.User, app);

            foreach (var (key, value) in httpContext.Request.Headers)
            {
                if (key.StartsWith("X-", StringComparison.OrdinalIgnoreCase))
                {
                    requestContext.Headers.Add(key, value.ToString());
                }
            }

            httpContext.Features.Set(requestContext);

            return requestContext;
        }

        private static bool HasPermission(string appName, Context requestContext)
        {
            return requestContext.Permissions.Includes(Permissions.ForApp(Permissions.App, appName));
        }

        private static bool AllowAnonymous(ActionExecutingContext context)
        {
            return context.ActionDescriptor.EndpointMetadata.Any(x => x is AllowAnonymousAttribute);
        }

        private static (string?, PermissionSet?) FindByOpenIdClient(IAppEntity app, ClaimsPrincipal user, bool isFrontend)
        {
            var (appName, clientId) = user.GetClient();

            if (app.Name != appName)
            {
                return (null, null);
            }

            if (clientId != null && app.Clients.TryGetValue(clientId, out var client) && app.Roles.TryGet(app.Name, client.Role, isFrontend, out var role))
            {
                return (client.Role, role.Permissions);
            }

            return (null, null);
        }

        private static (string?, PermissionSet?) FindAnonymousClient(IAppEntity app, bool isFrontend)
        {
            var client = app.Clients.Values.FirstOrDefault(x => x.AllowAnonymous);

            if (client != null && app.Roles.TryGet(app.Name, client.Role, isFrontend, out var role))
            {
                return (client.Role, role.Permissions);
            }

            return (null, null);
        }

        private static (string?, PermissionSet?) FindByOpenIdSubject(IAppEntity app, ClaimsPrincipal user, bool isFrontend)
        {
            var subjectId = user.OpenIdSubject();

            if (subjectId != null && app.Contributors.TryGetValue(subjectId, out var roleName) && app.Roles.TryGet(app.Name, roleName, isFrontend, out var role))
            {
                return (roleName, role.Permissions);
            }

            return (null, null);
        }
    }
}
