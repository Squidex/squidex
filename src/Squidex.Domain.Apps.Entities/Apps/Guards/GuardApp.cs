// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public static class GuardApp
    {
        public static Task CanCreate(CreateApp command, IAppProvider appProvider)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot create app.", async e =>
            {
                if (!command.Name.IsSlug())
                {
                    e("Name must be a valid slug.", nameof(command.Name));
                }
                else if (await appProvider.GetAppAsync(command.Name) != null)
                {
                    e("An app with the same name already exists.", nameof(command.Name));
                }
            });
        }

        public static void CanChangePlan(ChangePlan command, AppPlan plan, IAppPlansProvider appPlans)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot change plan.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.PlanId))
                {
                    e("Plan id is required.", nameof(command.PlanId));
                    return;
                }

                if (appPlans.GetPlan(command.PlanId) == null)
                {
                    e("A plan with this id does not exist.", nameof(command.PlanId));
                }

                if (!string.IsNullOrWhiteSpace(command.PlanId) && plan != null && !plan.Owner.Equals(command.Actor))
                {
                    e("Plan can only changed from the user who configured the plan initially.");
                }

                if (string.Equals(command.PlanId, plan?.PlanId, StringComparison.OrdinalIgnoreCase))
                {
                    e("App has already this plan.");
                }
            });
        }
    }
}
