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
using Squidex.Infrastructure.Security;
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

                var subject = context.HttpContext.User.FindFirst(OpenIdClaims.Subject)?.Value;

                if (subject == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                var contributor = app.Contributors.FirstOrDefault(x => string.Equals(x.ContributorId, subject, StringComparison.OrdinalIgnoreCase));

                if (contributor == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                var roleName = $"app-{contributor.Permission.ToString().ToLowerInvariant()}";

                var defaultIdentity = context.HttpContext.User.Identities.First();

                defaultIdentity
                    .AddClaim(
                        new Claim(defaultIdentity.RoleClaimType, roleName));

                context.HttpContext.Features.Set<IAppFeature>(new AppFeature(app));
            }
        }
    }
}
