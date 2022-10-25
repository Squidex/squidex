﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Plans.Models;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Plans
{
    /// <summary>
    /// Update and query plans.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Plans))]
    public sealed class AppPlansController : ApiController
    {
        private readonly IBillingPlans billingPlans;
        private readonly IBillingManager billingManager;
        private readonly IAppUsageGate appUsageGate;

        public AppPlansController(ICommandBus commandBus,
            IAppUsageGate appUsageGate,
            IBillingPlans billingPlans,
            IBillingManager billingManager)
            : base(commandBus)
        {
            this.billingPlans = billingPlans;
            this.billingManager = billingManager;
            this.appUsageGate = appUsageGate;
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
        [ProducesResponseType(typeof(PlansDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(PermissionIds.AppPlansRead)]
        [ApiCosts(0)]
        public IActionResult GetPlans(string app)
        {
            var response = Deferred.AsyncResponse(async () =>
            {
                var plans = billingPlans.GetAvailablePlans();

                var (plan, link, referral) =
                    await AsyncHelper.WhenAll(
                        appUsageGate.GetPlanForAppAsync(App, HttpContext.RequestAborted),
                        billingManager.GetPortalLinkAsync(UserId, App, HttpContext.RequestAborted),
                        billingManager.GetReferralCodeAsync(UserId, App, HttpContext.RequestAborted));

                var owner = App.Plan?.Owner.Identifier;
                var isOwner = string.Equals(owner, UserId, StringComparison.Ordinal);
                var isLocked = PlansLockedReason.None;

                if (plan.TeamId != null)
                {
                    isLocked = PlansLockedReason.ManagedByTeam;
                }
                else if (!Resources.CanChangePlan)
                {
                    isLocked = PlansLockedReason.NoPermission;
                }
                else if (owner != null && !isOwner)
                {
                    isLocked = PlansLockedReason.NotOwner;
                }

                var dto = PlansDto.FromDomain(plans.ToArray(), owner, plan.PlanId, isLocked);

                dto.PortalLink = isLocked == PlansLockedReason.None ? link : null;
                dto.ReferralCode = referral.Code;
                dto.ReferralEarned = $"{referral.AmountEarned:00} EUR";

                return dto;
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
        [ApiPermissionOrAnonymous(PermissionIds.AppPlansChange)]
        [ApiCosts(0)]
        public async Task<IActionResult> PutPlan(string app, [FromBody] ChangePlanDto request)
        {
            var command = SimpleMapper.Map(request, new ChangePlan());

            var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

            string? redirectUri = null;

            if (context.PlainResult is PlanChangedResult result)
            {
                redirectUri = result.RedirectUri?.ToString();
            }

            return Ok(new PlanChangedDto { RedirectUri = redirectUri });
        }
    }
}
