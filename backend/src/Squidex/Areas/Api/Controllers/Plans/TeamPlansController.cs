// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Plans.Models;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.Teams.Commands;
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
public sealed class TeamPlansController : ApiController
{
    private readonly IUsageGate appUsageGate;
    private readonly IBillingPlans billingPlans;
    private readonly IBillingManager billingManager;

    public TeamPlansController(ICommandBus commandBus,
        IUsageGate appUsageGate,
        IBillingPlans billingPlans,
        IBillingManager billingManager)
        : base(commandBus)
    {
        this.appUsageGate = appUsageGate;
        this.billingPlans = billingPlans;
        this.billingManager = billingManager;
    }

    /// <summary>
    /// Get team plan information.
    /// </summary>
    /// <param name="team">The name of the team.</param>
    /// <response code="200">Team plan information returned.</response>.
    /// <response code="404">Team not found.</response>.
    [HttpGet]
    [Route("teams/{team}/plans/")]
    [ProducesResponseType(typeof(PlansDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.TeamPlansRead)]
    [ApiCosts(0)]
    public IActionResult GetTeamPlans(string team)
    {
        var response = Deferred.AsyncResponse(async () =>
        {
            var plans = billingPlans.GetAvailablePlans();

            var (plan, link, referral) =
                await AsyncHelper.WhenAll(
                    appUsageGate.GetPlanForTeamAsync(Team, HttpContext.RequestAborted),
                    billingManager.GetPortalLinkAsync(UserId, Team, HttpContext.RequestAborted),
                    billingManager.GetReferralInfoAsync(UserId, Team, HttpContext.RequestAborted));

            PlansLockedReason GetLocked()
            {
                if (!Resources.CanChangeTeamPlan)
                {
                    return PlansLockedReason.NoPermission;
                }

                return PlansLockedReason.None;
            }

            return PlansDto.FromDomain(
                plans.ToArray(), null,
                plan.PlanId,
                referral,
                link,
                GetLocked());
        });

        Response.Headers[HeaderNames.ETag] = Team.ToEtag();

        return Ok(response);
    }

    /// <summary>
    /// Change the team plan.
    /// </summary>
    /// <param name="team">The name of the team.</param>
    /// <param name="request">Plan object that needs to be changed.</param>
    /// <response code="200">Plan changed or redirect url returned.</response>.
    /// <response code="404">Team not found.</response>.
    [HttpPut]
    [Route("teams/{team}/plan/")]
    [ProducesResponseType(typeof(PlanChangedDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.TeamPlansChange)]
    [ApiCosts(0)]
    public async Task<IActionResult> PutTeamPlan(string team, [FromBody] ChangePlanDto request)
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
