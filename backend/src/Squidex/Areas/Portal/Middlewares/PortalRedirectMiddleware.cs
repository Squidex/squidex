// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Domain.Apps.Entities.Billing;

namespace Squidex.Areas.Portal.Middlewares
{
    public sealed class PortalRedirectMiddleware
    {
        private readonly IBillingManager billingManager;

        public PortalRedirectMiddleware(RequestDelegate next, IBillingManager billingManager)
        {
            this.billingManager = billingManager;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/")
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim != null)
                {
                    var portalLink = await billingManager.GetPortalLinkAsync(userIdClaim.Value, context.RequestAborted);

                    context.Response.Redirect(portalLink);
                }
            }
        }
    }
}
