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
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Pipeline
{
    public sealed class AppResolver : IAsyncActionFilter
    {
        private readonly IAppProvider appProvider;

        public class AppFeature : IAppFeature
        {
            public IAppEntity App { get; }

            public AppFeature(IAppEntity app)
            {
                App = app;
            }
        }

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

                var permissions =
                    FindByOpenIdSubject(app, user) ??
                    FindByOpenIdClient(app, user);

                if (permissions.Count == 0)
                {
                    var set = new PermissionSet(user.Permissions().Select(x => new Permission(x)));

                    if (!set.Includes(Permissions.ForApp(Permissions.App, appName)))
                    {
                        context.Result = new NotFoundResult();
                        return;
                    }
                }

                var identity = user.Identities.First();

                foreach (var permission in permissions)
                {
                    identity.AddClaim(new Claim(SquidexClaimTypes.Permissions, permission.Id));
                }

                context.HttpContext.Features.Set<IAppFeature>(new AppFeature(app));
            }

            await next();
        }

        private static PermissionSet FindByOpenIdClient(IAppEntity app, ClaimsPrincipal user)
        {
            var clientId = user.GetClientId();

            if (clientId != null && app.Clients.TryGetValue(clientId, out var client))
            {
                return client.Permission.ToPermissions(app.Name);
            }

            return PermissionSet.Empty;
        }

        private static PermissionSet FindByOpenIdSubject(IAppEntity app, ClaimsPrincipal user)
        {
            var subjectId = user.OpenIdSubject();

            if (subjectId != null && app.Contributors.TryGetValue(subjectId, out var permission))
            {
                return permission.ToPermissions(app.Name);
            }

            return PermissionSet.Empty;
        }
    }
}
