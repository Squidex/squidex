// ==========================================================================
//  AppFilterAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Core.Apps;
using Squidex.Infrastructure.Security;
using Squidex.Read.Apps;
using Squidex.Read.Apps.Services;

namespace Squidex.Pipeline
{
    public sealed class AppFilterAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly IAppProvider appProvider;

        public AppFilterAttribute(IAppProvider appProvider)
        {
            this.appProvider = appProvider;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var appName = context.RouteData.Values["app"]?.ToString();

            if (!string.IsNullOrWhiteSpace(appName))
            {
                var app = await appProvider.FindAppByNameAsync(appName);

                if (app == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                var user = context.HttpContext.User;

                var permission =
                    FindByOpenIdSubject(app, user) ??
                    FindByOpenIdClient(app, user);

                if (permission == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                var roleName = $"app-{permission.ToString().ToLowerInvariant()}";

                var defaultIdentity = context.HttpContext.User.Identities.First();

                defaultIdentity
                    .AddClaim(
                        new Claim(defaultIdentity.RoleClaimType, roleName));

                context.HttpContext.Features.Set<IAppFeature>(new AppFeature(app));
            }
        }

        private static PermissionLevel? FindByOpenIdClient(IAppEntity app, ClaimsPrincipal user)
        {
            var clientId = user.FindFirst(OpenIdClaims.ClientId)?.Value;

            if (clientId == null)
            {
                return null;
            }

            clientId = clientId.Split(':')[0];

            var contributor = app.Clients.FirstOrDefault(x => string.Equals(x.ClientName, clientId, StringComparison.OrdinalIgnoreCase));

            return contributor != null ? PermissionLevel.Owner : PermissionLevel.Editor;
        }

        private static PermissionLevel? FindByOpenIdSubject(IAppEntity app, ClaimsPrincipal user)
        {
            var subject = user.FindFirst(OpenIdClaims.Subject)?.Value;

            if (subject == null)
            {
                return null;
            }

            var contributor = app.Contributors.FirstOrDefault(x => string.Equals(x.ContributorId, subject, StringComparison.OrdinalIgnoreCase));

            return contributor?.Permission;
        }
    }
}
