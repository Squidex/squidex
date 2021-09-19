// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public sealed class ConfigAppPlansProvider : IAppPlansProvider
    {
        private static readonly ConfigAppLimitsPlan Infinite = new ConfigAppLimitsPlan
        {
            Id = "infinite",
            Name = "Infinite",
            MaxApiCalls = -1,
            MaxAssetSize = -1,
            MaxContributors = -1,
            BlockingApiCalls = -1
        };

        private readonly Dictionary<string, ConfigAppLimitsPlan> plansById = new Dictionary<string, ConfigAppLimitsPlan>(StringComparer.OrdinalIgnoreCase);
        private readonly List<ConfigAppLimitsPlan> plansList = new List<ConfigAppLimitsPlan>();
        private readonly ConfigAppLimitsPlan freePlan;

        public ConfigAppPlansProvider(IEnumerable<ConfigAppLimitsPlan> config)
        {
            foreach (var plan in config.OrderBy(x => x.MaxApiCalls).Select(x => x.Clone()))
            {
                plansList.Add(plan);
                plansById[plan.Id] = plan;

                if (!string.IsNullOrWhiteSpace(plan.YearlyId) && !string.IsNullOrWhiteSpace(plan.YearlyCosts))
                {
                    plansById[plan.YearlyId] = plan;
                }
            }

            freePlan = plansList.Find(x => x.IsFree) ?? Infinite;
        }

        public IEnumerable<IAppLimitsPlan> GetAvailablePlans()
        {
            return plansList;
        }

        public bool IsConfiguredPlan(string? planId)
        {
            return planId != null && plansById.ContainsKey(planId);
        }

        public IAppLimitsPlan? GetPlan(string? planId)
        {
            return plansById.GetOrDefault(planId ?? string.Empty);
        }

        public IAppLimitsPlan GetFreePlan()
        {
            return freePlan;
        }

        public IAppLimitsPlan? GetPlanUpgradeForApp(IAppEntity app)
        {
            Guard.NotNull(app, nameof(app));

            return GetPlanUpgrade(app.Plan?.PlanId);
        }

        public IAppLimitsPlan? GetPlanUpgrade(string? planId)
        {
            var plan = GetPlanCore(planId);

            var nextPlanIndex = plansList.IndexOf(plan);

            if (nextPlanIndex >= 0 && nextPlanIndex < plansList.Count - 1)
            {
                return plansList[nextPlanIndex + 1];
            }

            return null;
        }

        public (IAppLimitsPlan Plan, string PlanId) GetPlanForApp(IAppEntity app)
        {
            Guard.NotNull(app, nameof(app));

            var planId = app.Plan?.PlanId;
            var plan = GetPlanCore(planId);

            if (plan.YearlyId != null && plan.YearlyId == planId)
            {
                return (plan, plan.YearlyId);
            }
            else
            {
                return (plan, plan.Id);
            }
        }

        private ConfigAppLimitsPlan GetPlanCore(string? planId)
        {
            return plansById.GetOrDefault(planId ?? string.Empty) ?? freePlan;
        }
    }
}
