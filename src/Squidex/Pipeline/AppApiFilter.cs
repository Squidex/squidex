// ==========================================================================
//  AppApiFilter.cs
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
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Shared.Identity;

// ReSharper disable SwitchStatementMissingSomeCases

namespace Squidex.Pipeline
{
    public sealed class AppApiFilter : IAsyncAuthorizationFilter
    {
        private readonly IAppProvider appProvider;
        private readonly IAppPlansProvider appPlanProvider;
        private readonly IUsageTracker usageTracker;

        public AppApiFilter(IAppProvider appProvider, IAppPlansProvider appPlanProvider, IUsageTracker usageTracker)
        {
            this.appProvider = appProvider;
            this.appPlanProvider = appPlanProvider;

            this.usageTracker = usageTracker;
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

                var plan = appPlanProvider.GetPlanForApp(app);

                var usage = await usageTracker.GetMonthlyCalls(app.Id.ToString(), DateTime.Today);

                if (plan.MaxApiCalls >= 0 && (usage * 1.1) > plan.MaxApiCalls)
                {
                    context.Result = new StatusCodeResult(429);
                    return;
                }
                
                var defaultIdentity = context.HttpContext.User.Identities.First();

                switch (permission.Value)
                {
                    case PermissionLevel.Owner:
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppOwner));
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppDeveloper));
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppEditor));
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppReader));
                        break;
                    case PermissionLevel.Developer:
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppDeveloper));
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppEditor));
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppReader));
                        break;
                    case PermissionLevel.Editor:
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppEditor));
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppReader));
                        break;
                    case PermissionLevel.Reader:
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppReader));
                        break;
                }

                context.HttpContext.Features.Set<IAppFeature>(new AppFeature(app));
            }
        }

        private static PermissionLevel? FindByOpenIdClient(IAppEntity app, ClaimsPrincipal user)
        {
            var client = app.Clients.FirstOrDefault(x => string.Equals(x.Id, user.GetClientId(), StringComparison.OrdinalIgnoreCase));

            if (client == null)
            {
                return null;
            }

            return client.IsReader ? PermissionLevel.Reader : PermissionLevel.Editor;
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
