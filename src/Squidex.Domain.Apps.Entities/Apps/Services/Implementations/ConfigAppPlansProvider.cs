// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Services.Implementations
{
    public sealed class ConfigAppPlansProvider : IAppPlansProvider
    {
        private static readonly ConfigAppLimitsPlan Infinite = new ConfigAppLimitsPlan
        {
            Id = "infinite",
            Name = "Infinite",
            MaxApiCalls = -1,
            MaxAssetSize = -1,
            MaxContributors = -1
        };

        private readonly Dictionary<string, ConfigAppLimitsPlan> plansById;
        private readonly List<ConfigAppLimitsPlan> plansList;

        public ConfigAppPlansProvider(IEnumerable<ConfigAppLimitsPlan> config)
        {
            Guard.NotNull(config, nameof(config));

            plansList = config.Select(c => c.Clone()).OrderBy(x => x.MaxApiCalls).ToList();
            plansById = plansList.ToDictionary(c => c.Id, StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<IAppLimitsPlan> GetAvailablePlans()
        {
            return plansList;
        }

        public bool IsConfiguredPlan(string planId)
        {
            return planId != null && plansById.ContainsKey(planId);
        }

        public IAppLimitsPlan GetPlanForApp(IAppEntity app)
        {
            Guard.NotNull(app, nameof(app));

            return GetPlan(app.Plan?.PlanId);
        }

        public IAppLimitsPlan GetPlan(string planId)
        {
            return GetPlanCore(planId);
        }

        public IAppLimitsPlan GetPlanUpgradeForApp(IAppEntity app)
        {
            Guard.NotNull(app, nameof(app));

            return GetPlanUpgrade(app.Plan?.PlanId);
        }

        public IAppLimitsPlan GetPlanUpgrade(string planId)
        {
            var plan = GetPlanCore(planId);

            var nextPlanIndex = plansList.IndexOf(plan);

            if (nextPlanIndex >= 0 && nextPlanIndex < plansList.Count - 1)
            {
                return plansList[nextPlanIndex + 1];
            }

            return null;
        }

        private ConfigAppLimitsPlan GetPlanCore(string planId)
        {
            return plansById.GetOrDefault(planId ?? string.Empty) ?? plansById.Values.FirstOrDefault() ?? Infinite;
        }
    }
}
