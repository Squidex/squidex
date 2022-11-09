// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Billing;

public sealed class ConfigPlansProvider : IBillingPlans
{
    private static readonly Plan Infinite = new Plan
    {
        Id = "infinite",
        Name = "Infinite",
        MaxApiCalls = -1,
        MaxAssetSize = -1,
        MaxContributors = -1,
        BlockingApiCalls = -1
    };

    private readonly Dictionary<string, Plan> plansById = new Dictionary<string, Plan>(StringComparer.OrdinalIgnoreCase);
    private readonly List<Plan> plans = new List<Plan>();
    private readonly Plan freePlan;

    public ConfigPlansProvider(IEnumerable<Plan> config)
    {
        plans.AddRange(config.OrderBy(x => x.MaxApiCalls));

        foreach (var plan in config.OrderBy(x => x.MaxApiCalls))
        {
            plansById[plan.Id] = plan;

            if (!string.IsNullOrWhiteSpace(plan.YearlyId) && !string.IsNullOrWhiteSpace(plan.YearlyCosts))
            {
                plansById[plan.YearlyId] = plan;
            }
        }

        freePlan = config.FirstOrDefault(x => x.IsFree) ?? Infinite;
    }

    public IEnumerable<Plan> GetAvailablePlans()
    {
        return plans;
    }

    public bool IsConfiguredPlan(string? planId)
    {
        return planId != null && plansById.ContainsKey(planId);
    }

    public Plan? GetPlan(string? planId)
    {
        return plansById.GetValueOrDefault(planId ?? string.Empty);
    }

    public Plan GetFreePlan()
    {
        return freePlan;
    }

    public (Plan Plan, string PlanId) GetActualPlan(string? planId)
    {
        if (planId == null || !plansById.TryGetValue(planId, out var plan))
        {
            var result = GetFreePlan();

            return (result, result.Id);
        }

        if (plan.YearlyId != null && plan.YearlyId == planId)
        {
            return (plan, plan.YearlyId);
        }
        else
        {
            return (plan, plan.Id);
        }
    }
}
