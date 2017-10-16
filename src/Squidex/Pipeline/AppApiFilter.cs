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

namespace Squidex.Pipeline
{
    public sealed class AppApiFilter : IAsyncAuthorizationFilter, IFilterContainer
    {
        private readonly IAppProvider appProvider;
        private readonly IAppPlansProvider appPlanProvider;
        private readonly IUsageTracker usageTracker;

        IFilterMetadata IFilterContainer.FilterDefinition { get; set; }

        public AppApiAttribute FilterDefinition
        {
            get
            {
                return (AppApiAttribute)((IFilterContainer)this).FilterDefinition;
            }
        }

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

                if (!FilterDefinition.CheckPermissions)
                {
                    context.HttpContext.Features.Set<IAppFeature>(new AppFeature(app));
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
                    case AppPermission.Owner:
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppOwner));
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppDeveloper));
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppEditor));
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppReader));
                        break;
                    case AppPermission.Developer:
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppDeveloper));
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppEditor));
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppReader));
                        break;
                    case AppPermission.Editor:
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppEditor));
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppReader));
                        break;
                    case AppPermission.Reader:
                        defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, SquidexRoles.AppReader));
                        break;
                }

                context.HttpContext.Features.Set<IAppFeature>(new AppFeature(app));
            }
        }

        private static AppPermission? FindByOpenIdClient(IAppEntity app, ClaimsPrincipal user)
        {
            var clientId = user.GetClientId();

            if (clientId != null && app.Clients.TryGetValue(clientId, out var client))
            {
                return client.Permission.ToAppPermission();
            }

            return null;
        }

        private static AppPermission? FindByOpenIdSubject(IAppEntity app, ClaimsPrincipal user)
        {
            var subjectId = user.FindFirst(OpenIdClaims.Subject)?.Value;

            if (subjectId != null && app.Contributors.TryGetValue(subjectId, out var contributor))
            {
                return contributor.Permission.ToAppPermission();
            }

            return null;
        }
    }
}
