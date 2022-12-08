// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Billing;

public interface IBillingPlans
{
    IEnumerable<Plan> GetAvailablePlans();

    bool IsConfiguredPlan(string? planId);

    Plan? GetPlan(string? planId);

    Plan GetFreePlan();

    (Plan Plan, string PlanId) GetActualPlan(string? planId);
}
