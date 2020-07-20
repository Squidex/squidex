// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Runner
{
    public sealed class GrainRuleRunnerService : IRuleRunnerService
    {
        private readonly IGrainFactory grainFactory;

        public GrainRuleRunnerService(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public Task CancelAsync(DomainId appId)
        {
            var grain = grainFactory.GetGrain<IRuleRunnerGrain>(appId.ToString());

            return grain.CancelAsync();
        }

        public Task<DomainId?> GetRunningRuleIdAsync(DomainId appId)
        {
            var grain = grainFactory.GetGrain<IRuleRunnerGrain>(appId.ToString());

            return grain.GetRunningRuleIdAsync();
        }

        public Task RunAsync(DomainId appId, DomainId ruleId)
        {
            var grain = grainFactory.GetGrain<IRuleRunnerGrain>(appId.ToString());

            return grain.RunAsync(ruleId);
        }
    }
}