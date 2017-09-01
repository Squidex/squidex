// ==========================================================================
//  PortalController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Infrastructure.Security;

namespace Squidex.Controllers.UI.Profile
{
    [Authorize]
    [SwaggerIgnore]
    public sealed class PortalController : Controller
    {
        private readonly IAppPlanBillingManager appPlansBillingManager;

        public PortalController(IAppPlanBillingManager appPlansBillingManager)
        {
            this.appPlansBillingManager = appPlansBillingManager;
        }

        [HttpGet]
        [Route("/account/portal")]
        public async Task<IActionResult> Portal()
        {
            var userId = User.FindFirst(OpenIdClaims.Subject).Value;

            var redirectUrl = await appPlansBillingManager.GetPortalLinkAsync(userId);

            return Redirect(redirectUrl);
        }
    }
}
