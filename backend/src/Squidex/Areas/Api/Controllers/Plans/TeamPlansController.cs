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
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Plans
{
    /// <summary>
    /// Update and query plans.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Plans))]
    public sealed class TeamPlansController : ApiController
    {
        private readonly IBillingPlans billingPlans;
        private readonly IBillingManager billingManager;

        public TeamPlansController(ICommandBus commandBus,
            IBillingPlans billingPlans,
            IBillingManager billingManager)
            : base(commandBus)
        {
            this.billingPlans = billingPlans;
            this.billingManager = billingManager;
        }

        /// <summary>
        /// Get team plan information.
        /// </summary>
        /// <param name="team">The name of the team.</param>
        /// <returns>
        /// 200 => Team plan information returned.
        /// 404 => Team not found.
        /// </returns>
        [HttpGet]
        [Route("teams/{team}/plans/")]
        [ProducesResponseType(typeof(PlansDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(PermissionIds.TeamPlansRead)]
        [ApiCosts(0)]
        public IActionResult GetPlans(string team)
        {
            var hasPortal = billingManager.HasPortal;

            var response = Deferred.Response(() =>
            {
                return PlansDto.FromDomain(Team, billingPlans, hasPortal);
            });

            Response.Headers[HeaderNames.ETag] = Team.ToEtag();

            return Ok(response);
        }

        /// <summary>
        /// Change the team plan.
        /// </summary>
        /// <param name="team">The name of the team.</param>
        /// <param name="request">Plan object that needs to be changed.</param>
        /// <returns>
        /// 200 => Plan changed or redirect url returned.
        /// 404 => Team not found.
        /// </returns>
        [HttpPut]
        [Route("teams/{team}/plan/")]
        [ProducesResponseType(typeof(PlanChangedDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(PermissionIds.TeamPlansChange)]
        [ApiCosts(0)]
        public async Task<IActionResult> PutPlan(string team, [FromBody] ChangePlanDto request)
        {
            var context = await CommandBus.PublishAsync(SimpleMapper.Map(this, new ChangePlan()), HttpContext.RequestAborted);

            string? redirectUri = null;

            if (context.PlainResult is PlanChangedResult result)
            {
                redirectUri = result.RedirectUri?.ToString();
            }

            return Ok(new PlanChangedDto { RedirectUri = redirectUri });
        }
    }
}
