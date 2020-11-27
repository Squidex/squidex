// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Runner
{
    public sealed class GrainRuleRunnerService : IRuleRunnerService
    {
        private readonly IGrainFactory grainFactory;
        private readonly IRuleService ruleService;

        public GrainRuleRunnerService(IGrainFactory grainFactory, IRuleService ruleService)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));
            Guard.NotNull(ruleService, nameof(ruleService));

            this.grainFactory = grainFactory;
            this.ruleService = ruleService;
        }

        public Task CancelAsync(DomainId appId)
        {
            var grain = grainFactory.GetGrain<IRuleRunnerGrain>(appId.ToString());

            return grain.CancelAsync();
        }

        public bool CanRunRule(IRuleEntity rule)
        {
            return rule.RuleDef.IsEnabled && rule.RuleDef.Trigger is not ManualTrigger;
        }

        public bool CanRunFromSnapshots(IRuleEntity rule)
        {
            return CanRunRule(rule) && ruleService.CanCreateSnapshotEvents(rule.RuleDef);
        }

        public Task<DomainId?> GetRunningRuleIdAsync(DomainId appId)
        {
            var grain = grainFactory.GetGrain<IRuleRunnerGrain>(appId.ToString());

            return grain.GetRunningRuleIdAsync();
        }

        public Task RunAsync(DomainId appId, DomainId ruleId, bool fromSnapshots = false)
        {
            var grain = grainFactory.GetGrain<IRuleRunnerGrain>(appId.ToString());

            return grain.RunAsync(ruleId, fromSnapshots);
        }
    }
}