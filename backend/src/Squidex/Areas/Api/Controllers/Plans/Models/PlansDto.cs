// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Plans.Models
{
    public sealed class PlansDto
    {
        /// <summary>
        /// The available plans.
        /// </summary>
        [LocalizedRequired]
        public PlanDto[] Plans { get; set; }

        /// <summary>
        /// The current plan id.
        /// </summary>
        public string? CurrentPlanId { get; set; }

        /// <summary>
        /// The plan owner.
        /// </summary>
        public string? PlanOwner { get; set; }

        /// <summary>
        /// The ID of the team.
        /// </summary>
        public DomainId? TeamId { get; set; }

        /// <summary>
        /// Indicates if there is a billing portal.
        /// </summary>
        public bool HasPortal { get; set; }

        public static PlansDto FromDomain(IAppEntity app, IBillingPlans plans, string planId, bool hasPortal)
        {
            var result = new PlansDto
            {
                CurrentPlanId = planId,
                Plans = plans.GetAvailablePlans().Select(PlanDto.FromDomain).ToArray(),
                PlanOwner = app.Plan?.Owner.Identifier,
                HasPortal = hasPortal,
                TeamId = app.TeamId
            };

            return result;
        }

        public static PlansDto FromDomain(ITeamEntity team, IBillingPlans plans, string planId, bool hasPortal)
        {
            var result = new PlansDto
            {
                CurrentPlanId = planId,
                Plans = plans.GetAvailablePlans().Select(PlanDto.FromDomain).ToArray(),
                PlanOwner = team.Plan?.Owner.Identifier,
                HasPortal = hasPortal
            };

            return result;
        }
    }
}
