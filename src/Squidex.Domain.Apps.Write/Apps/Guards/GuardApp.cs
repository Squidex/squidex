// ==========================================================================
//  GuardApp.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Guards
{
    public static class GuardApp
    {
        public static Task CanCreate(CreateApp command, IAppProvider apps)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot create app.", async error =>
            {
                if (await apps.FindAppByNameAsync(command.Name) != null)
                {
                    error(new ValidationError($"An app with name '{command.Name}' already exists", nameof(command.Name)));
                }

                if (!command.Name.IsSlug())
                {
                    error(new ValidationError("Name must be a valid slug.", nameof(command.Name)));
                }
            });
        }

        public static void CanChangePlan(ChangePlan command, AppPlan plan, IAppPlansProvider appPlans)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot change plan.", error =>
            {
                if (string.IsNullOrWhiteSpace(command.PlanId))
                {
                    error(new ValidationError("PlanId is not defined.", nameof(command.PlanId)));
                }
                else if (appPlans.GetPlan(command.PlanId) == null)
                {
                    error(new ValidationError("Plan id not available.", nameof(command.PlanId)));
                }

                if (!string.IsNullOrWhiteSpace(command.PlanId) && plan != null && !plan.Owner.Equals(command.Actor))
                {
                    error(new ValidationError("Plan can only be changed from current user."));
                }

                if (string.Equals(command.PlanId, plan?.PlanId, StringComparison.OrdinalIgnoreCase))
                {
                    error(new ValidationError("App has already this plan."));
                }
            });
        }
    }
}
