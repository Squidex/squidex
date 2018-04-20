// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Services;

namespace Squidex.Areas.Api.Controllers.Plans.Models
{
    public sealed class AppPlansDto
    {
        /// <summary>
        /// The available plans.
        /// </summary>
        [Required]
        public List<PlanDto> Plans { get; set; }

        /// <summary>
        /// The current plan id.
        /// </summary>
        public string CurrentPlanId { get; set; }

        /// <summary>
        /// The plan owner.
        /// </summary>
        public string PlanOwner { get; set; }

        /// <summary>
        /// Indicates if there is a billing portal.
        /// </summary>
        public bool HasPortal { get; set; }

        public static AppPlansDto FromApp(IAppEntity app, IAppPlansProvider plans, bool hasPortal)
        {
            var planId = plans.GetPlanForApp(app).Id;

            var response = new AppPlansDto
            {
                CurrentPlanId = planId,
                Plans = plans.GetAvailablePlans().Select(PlanDto.FromPlan).ToList(),
                PlanOwner = app.Plan?.Owner.Identifier,
                HasPortal = hasPortal
            };

            return response;
        }
    }
}
