// ==========================================================================
//  PortalRedirectMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Infrastructure.Security;

namespace Squidex.Areas.Portal.Middlewares
{
    public sealed class PortalRedirectMiddleware
    {
        private readonly IAppPlanBillingManager appPlansBillingManager;
        private readonly RequestDelegate next;

        public PortalRedirectMiddleware(RequestDelegate next, IAppPlanBillingManager appPlansBillingManager)
        {
            this.next = next;

            this.appPlansBillingManager = appPlansBillingManager;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/")
            {
                var userId = context.User.FindFirst(OpenIdClaims.Subject).Value;

                context.Response.RedirectToAbsoluteUrl(await appPlansBillingManager.GetPortalLinkAsync(userId));
            }
        }
    }
}
