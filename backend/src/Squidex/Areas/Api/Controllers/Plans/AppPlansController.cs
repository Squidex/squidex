// ==========================================================================
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

namespace Squidex.Areas.Api.Controllers.Plans;

/// <summary>
/// Update and query plans.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Plans))]
public sealed class AppPlansController : ApiController
{
    private readonly IBillingPlans billingPlans;
    private readonly IBillingManager billingManager;
    private readonly IUsageGate usageGate;

    public AppPlansController(ICommandBus commandBus,
        IUsageGate usageGate,
        IBillingPlans billingPlans,
        IBillingManager billingManager)
        : base(commandBus)
    {
        this.billingPlans = billingPlans;
        this.billingManager = billingManager;
        this.usageGate = usageGate;
    }

    /// <summary>
    /// Get app plan information.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">App plan information returned.</response>.
    /// <response code="404">App not found.</response>.
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
                    usageGate.GetPlanForAppAsync(App, false, HttpContext.RequestAborted),
                    billingManager.GetPortalLinkAsync(UserId, App, HttpContext.RequestAborted),
                    billingManager.GetReferralInfoAsync(UserId, App, HttpContext.RequestAborted));

            var planOwner = App.Plan?.Owner.Identifier;

            PlansLockedReason GetLocked()
            {
                if (plan.TeamId != null)
                {
                    return PlansLockedReason.ManagedByTeam;
                }
                else if (!Resources.CanChangePlan)
                {
                    return PlansLockedReason.NoPermission;
                }
                else if (planOwner != null && !string.Equals(planOwner, UserId, StringComparison.Ordinal))
                {
                    return PlansLockedReason.NotOwner;
                }

                return PlansLockedReason.None;
            }

            return PlansDto.FromDomain(
                plans.ToArray(),
                planOwner,
                plan.PlanId,
                referral,
                link,
                GetLocked());
        });

        Response.Headers[HeaderNames.ETag] = App.ToEtag();

        return Ok(response);
    }

    /// <summary>
    /// Change the app plan.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">Plan object that needs to be changed.</param>
    /// <response code="200">Plan changed or redirect url returned.</response>.
    /// <response code="400">Plan not owned by user.</response>.
    /// <response code="404">App not found.</response>.
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
