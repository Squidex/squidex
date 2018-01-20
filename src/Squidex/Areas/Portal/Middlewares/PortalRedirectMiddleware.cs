// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities.Apps.Services;

namespace Squidex.Areas.Portal.Middlewares
{
    public sealed class PortalRedirectMiddleware
    {
        private readonly IAppPlanBillingManager appPlansBillingManager;

        public PortalRedirectMiddleware(RequestDelegate next, IAppPlanBillingManager appPlansBillingManager)
        {
            this.appPlansBillingManager = appPlansBillingManager;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/")
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim != null)
                {
                    context.Response.RedirectToAbsoluteUrl(await appPlansBillingManager.GetPortalLinkAsync(userIdClaim.Value));
                }
            }
        }
    }
}
