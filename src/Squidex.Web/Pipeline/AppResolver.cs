// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
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
            this.appProvider = appProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            var appName = context.RouteData.Values["app"]?.ToString();

            if (!string.IsNullOrWhiteSpace(appName))
            {
                var app = await appProvider.GetAppAsync(appName);

                if (app == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                var (role, permissions) = FindByOpenIdSubject(app, user);

                if (permissions == null)
                {
                    (role, permissions) = FindByOpenIdClient(app, user);
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

                var permissionSet = user.Permissions();

                context.HttpContext.Context().App = app;

                if (!permissionSet.Includes(Permissions.ForApp(Permissions.App, appName)) && !AllowAnonymous(context))
                {
                    context.Result = new NotFoundResult();
                    return;
                }
            }

            await next();
        }

        private static bool AllowAnonymous(ActionExecutingContext context)
        {
            return context.ActionDescriptor.FilterDescriptors.Any(x => x.Filter is AllowAnonymousFilter);
        }

        private static (string, PermissionSet) FindByOpenIdClient(IAppEntity app, ClaimsPrincipal user)
        {
            var clientId = user.GetClientId();

            if (clientId != null && app.Clients.TryGetValue(clientId, out var client) && app.Roles.TryGetValue(client.Role, out var role))
            {
                return (client.Role, role.Permissions);
            }

            return (null, null);
        }

        private static (string, PermissionSet) FindByOpenIdSubject(IAppEntity app, ClaimsPrincipal user)
        {
            var subjectId = user.OpenIdSubject();

            if (subjectId != null && app.Contributors.TryGetValue(subjectId, out var roleName) && app.Roles.TryGetValue(roleName, out var role))
            {
                return (roleName, role.Permissions);
            }

            return (null, null);
        }
    }
}
