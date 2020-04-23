// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
            Guard.NotNull(grainFactory);

            this.grainFactory = grainFactory;
        }

        public Task CancelAsync(Guid appId)
        {
            var grain = grainFactory.GetGrain<IRuleRunnerGrain>(appId);

            return grain.CancelAsync();
        }

        public Task<Guid?> GetRunningRuleIdAsync(Guid appId)
        {
            var grain = grainFactory.GetGrain<IRuleRunnerGrain>(appId);

            return grain.GetRunningRuleIdAsync();
        }

        public Task RunAsync(Guid appId, Guid ruleId)
        {
            var grain = grainFactory.GetGrain<IRuleRunnerGrain>(appId);

            return grain.RunAsync(ruleId);
        }
    }
}