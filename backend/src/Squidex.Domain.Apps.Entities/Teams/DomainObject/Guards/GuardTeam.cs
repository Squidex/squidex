// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.Teams.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Teams.DomainObject.Guards;

public static class GuardTeam
{
    public static void CanCreate(CreateTeam command)
    {
        Guard.NotNull(command);

        Validate.It(e =>
        {
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                e(Not.Defined(nameof(command.Name)), nameof(command.Name));
            }
        });
    }

    public static void CanUpdate(UpdateTeam command)
    {
        Guard.NotNull(command);

        Validate.It(e =>
        {
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                e(Not.Defined(nameof(command.Name)), nameof(command.Name));
            }
        });
    }

    public static void CanChangePlan(ChangePlan command, IBillingPlans billingPlans)
    {
        Guard.NotNull(command);

        Validate.It(e =>
        {
            if (string.IsNullOrWhiteSpace(command.PlanId))
            {
                e(Not.Defined(nameof(command.PlanId)), nameof(command.PlanId));
                return;
            }

            if (billingPlans.GetPlan(command.PlanId) == null)
            {
                e(T.Get("apps.plans.notFound"), nameof(command.PlanId));
            }
        });
    }
}
