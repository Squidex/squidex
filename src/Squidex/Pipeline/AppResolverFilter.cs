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

namespace Squidex.Pipeline
{
    public sealed class AppResolverFilter : IAsyncActionFilter
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

        public AppResolverFilter(IAppProvider appProvider)
        {
            this.appProvider = appProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var appName = context.RouteData.Values["app"]?.ToString();

            if (!string.IsNullOrWhiteSpace(appName))
            {
                var app = await appProvider.GetAppAsync(appName);

                if (app == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                var user = context.HttpContext.User;

                var permissions =
                    FindByOpenIdSubject(app, user) ??
                    FindByOpenIdClient(app, user);

                if (permissions.Count == 0)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                var identity = user.Identities.First();

                foreach (var permission in permissions)
                {
                    identity.AddClaim(new Claim("Permission", permission.Id));
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
            var subjectId = user.FindFirst(OpenIdClaims.Subject)?.Value;

            if (subjectId != null && app.Contributors.TryGetValue(subjectId, out var permission))
            {
                return permission.ToPermissions(app.Name);
            }

            return PermissionSet.Empty;
        }
    }
}
