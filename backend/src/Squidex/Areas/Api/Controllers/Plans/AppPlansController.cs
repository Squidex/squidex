// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Plans.Models;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Plans
{
    /// <summary>
    /// Manages and configures plans.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Plans))]
    public sealed class AppPlansController : ApiController
    {
        private readonly IAppPlansProvider appPlansProvider;
        private readonly IAppPlanBillingManager appPlansBillingManager;

        public AppPlansController(ICommandBus commandBus,
            IAppPlansProvider appPlansProvider,
            IAppPlanBillingManager appPlansBillingManager)
            : base(commandBus)
        {
            this.appPlansProvider = appPlansProvider;
            this.appPlansBillingManager = appPlansBillingManager;
        }

        /// <summary>
        /// Get app plan information.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => App plan information returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/plans/")]
        [ProducesResponseType(typeof(AppPlansDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppPlansRead)]
        [ApiCosts(0)]
        public IActionResult GetPlans(string app)
        {
            var hasPortal = appPlansBillingManager.HasPortal;

            var response = Deferred.Response(() =>
            {
                return AppPlansDto.FromApp(App, appPlansProvider, hasPortal);
            });

            Response.Headers[HeaderNames.ETag] = App.ToEtag();

            return Ok(response);
        }

        /// <summary>
        /// Change the app plan.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">Plan object that needs to be changed.</param>
        /// <returns>
        /// 200 => Plan changed or redirect url returned.
        /// 400 => Plan not owned by user.
        /// 404 => App not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/plan/")]
        [ProducesResponseType(typeof(PlanChangedDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppPlansChange)]
        [ApiCosts(0)]
        public async Task<IActionResult> PutPlan(string app, [FromBody] ChangePlanDto request)
        {
            var context = await CommandBus.PublishAsync(request.ToCommand(HttpContext));

            string? redirectUri = null;

            if (context.PlainResult is RedirectToCheckoutResult result)
            {
                redirectUri = result.Url.ToString();
            }

            return Ok(new PlanChangedDto { RedirectUri = redirectUri });
        }
    }
}
