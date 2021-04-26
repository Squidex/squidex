// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Plans.Models
{
    public sealed class AppPlansDto
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
        /// Indicates if there is a billing portal.
        /// </summary>
        public bool HasPortal { get; set; }

        public static AppPlansDto FromApp(IAppEntity app, IAppPlansProvider plans, bool hasPortal)
        {
            var (_, planId) = plans.GetPlanForApp(app);

            var response = new AppPlansDto
            {
                CurrentPlanId = planId,
                Plans = plans.GetAvailablePlans().Select(PlanDto.FromPlan).ToArray(),
                PlanOwner = app.Plan?.Owner.Identifier,
                HasPortal = hasPortal
            };

            return response;
        }
    }
}
