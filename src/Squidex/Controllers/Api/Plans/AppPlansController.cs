// ==========================================================================
//  AppPlansController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NSwag.Annotations;
using Squidex.Controllers.Api.Plans.Models;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;
using Squidex.Pipeline;

// ReSharper disable RedundantIfElseBlock

namespace Squidex.Controllers.Api.Plans
{
    /// <summary>
    /// Manages and configures plans.
    /// </summary>
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag("Plans")]
    public class AppPlansController : ControllerBase
    {
        private readonly IAppPlansProvider appPlansProvider;
        private readonly IAppPlanBillingManager appPlansBillingManager;

        public AppPlansController(ICommandBus commandBus, IAppPlansProvider appPlansProvider, IAppPlanBillingManager appPlansBillingManager)
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
        [MustBeAppOwner]
        [HttpGet]
        [Route("apps/{app}/plans/")]
        [ProducesResponseType(typeof(AppPlansDto), 200)]
        [ApiCosts(0.5)]
        public async Task<IActionResult> GetPlans(string app)
        {
            var userId = User.FindFirst(OpenIdClaims.Subject).Value;

            var planId = appPlansProvider.GetPlanForApp(App).Id;

            var hasPortal = appPlansBillingManager.HasPortal;
            var hasConfigured = await appPlansBillingManager.HasPaymentOptionsAsync(userId);

            var response = new AppPlansDto
            {
                Plans = appPlansProvider.GetAvailablePlans().Select(x => SimpleMapper.Map(x, new PlanDto())).ToList(),
                PlanOwner = App.PlanOwner,
                HasPortal = hasPortal,
                HasConfigured = hasConfigured,
                CurrentPlanId = planId
            };

            Response.Headers["ETag"] = new StringValues(App.Version.ToString());

            return Ok(response);
        }

        /// <summary>
        /// Change the app plan.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">Plan object that needs to be changed.</param>
        /// <returns>
        /// 201 => Redirected to checkout page.
        /// 204 => Plan changed.
        /// 400 => Plan not owned by user.
        /// 404 => App not found.
        /// </returns>
        [MustBeAppOwner]
        [HttpPut]
        [Route("apps/{app}/plan/")]
        [ProducesResponseType(typeof(PlanChangedDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(0.5)]
        public async Task<IActionResult> ChangePlanAsync(string app, [FromBody] ChangePlanDto request)
        {
            var redirectUri = (string)null;
            var context = await CommandBus.PublishAsync(SimpleMapper.Map(request, new ChangePlan()));

            if (context.Result<object>() is RedirectToCheckoutResult result)
            {
                redirectUri = result.Url.ToString();
            }

            return Ok(new PlanChangedDto { RedirectUri = redirectUri });
        }
    }
}
