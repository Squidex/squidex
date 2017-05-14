// ==========================================================================
//  ConfigAppLimitsProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Read.Apps.Services.Implementations
{
    public sealed class ConfigAppLimitsProvider : IAppLimitsProvider
    {
        private static readonly ConfigAppLimitsPlan Infinite = new ConfigAppLimitsPlan
        {
            Name = "Infinite",
            MaxApiCalls = -1,
            MaxAssetSize = -1,
            MaxContributors = -1
        };

        private readonly List<ConfigAppLimitsPlan> config;

        public ConfigAppLimitsProvider(IEnumerable<ConfigAppLimitsPlan> config)
        {
            Guard.NotNull(config, nameof(config));

            this.config = config.Select(c => c.Clone()).OrderBy(x => x.MaxApiCalls).ToList();
        }

        public IEnumerable<IAppLimitsPlan> GetAvailablePlans()
        {
            return config;
        }

        public IAppLimitsPlan GetPlanForApp(IAppEntity app)
        {
            Guard.NotNull(app, nameof(app));

            return GetPlan(app.PlanId);
        }

        public IAppLimitsPlan GetPlan(int planId)
        {
            if (planId >= 0 && planId < config.Count)
            {
                return config[planId];
            }

            return config.FirstOrDefault() ?? Infinite;
        }
    }
}
