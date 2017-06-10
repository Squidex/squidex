// ==========================================================================
//  ConfigAppLimitsProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Read.Apps.Services.Implementations
{
    public sealed class ConfigAppPlansProvider : IAppPlansProvider
    {
        private static readonly ConfigAppLimitsPlan Infinite = new ConfigAppLimitsPlan
        {
            Name = "Infinite",
            MaxApiCalls = -1,
            MaxAssetSize = -1,
            MaxContributors = -1
        };

        private readonly Dictionary<string, ConfigAppLimitsPlan> config;

        public ConfigAppPlansProvider(IEnumerable<ConfigAppLimitsPlan> config)
        {
            Guard.NotNull(config, nameof(config));

            this.config = config.Select(c => c.Clone()).OrderBy(x => x.MaxApiCalls).ToDictionary(c => c.Id, StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<IAppLimitsPlan> GetAvailablePlans()
        {
            return config.Values;
        }

        public IAppLimitsPlan GetPlanForApp(IAppEntity app)
        {
            Guard.NotNull(app, nameof(app));

            return GetPlan(app.PlanId);
        }

        public IAppLimitsPlan GetPlan(string planId)
        {
            return config.GetOrDefault(planId ?? string.Empty) ?? config.Values.First() ?? Infinite;
        }

        public bool IsConfiguredPlan(string planId)
        {
            return planId != null && config.ContainsKey(planId);
        }
    }
}
