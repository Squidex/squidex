// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Orleans;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules.Runner
{
    public sealed class DefaultRuleRunnerService : IRuleRunnerService
    {
        private const int MaxSimulatedEvents = 100;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly IEventStore eventStore;
        private readonly IGrainFactory grainFactory;
        private readonly IRuleService ruleService;

        public DefaultRuleRunnerService(IGrainFactory grainFactory,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            IRuleService ruleService)
        {
            this.grainFactory = grainFactory;
            this.eventDataFormatter = eventDataFormatter;
            this.eventStore = eventStore;
            this.ruleService = ruleService;
        }

        public async Task<List<SimulatedRuleEvent>> SimulateAsync(IRuleEntity rule, CancellationToken ct)
        {
            Guard.NotNull(rule, nameof(rule));

            var context = GetContext(rule);

            var result = new List<SimulatedRuleEvent>(MaxSimulatedEvents);

            var fromNow = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(7));

            await foreach (var storedEvent in eventStore.QueryAllReverseAsync($"^([a-z]+)\\-{rule.AppId.Id}", fromNow, MaxSimulatedEvents, ct))
            {
                var @event = eventDataFormatter.ParseIfKnown(storedEvent);

                if (@event?.Payload is AppEvent appEvent)
                {
                    await foreach (var (job, exception, skip) in ruleService.CreateJobsAsync(@event, context, ct))
                    {
                        var name = job?.EventName;

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            name = ruleService.GetName(appEvent);
                        }

                        var simulationResult = new SimulatedRuleEvent(
                            name,
                            job?.ActionName,
                            job?.ActionData,
                            exception?.Message,
                            skip);

                        result.Add(simulationResult);
                    }
                }
            }

            return result;
        }

        public Task CancelAsync(DomainId appId)
        {
            var grain = grainFactory.GetGrain<IRuleRunnerGrain>(appId.ToString());

            return grain.CancelAsync();
        }

        public bool CanRunRule(IRuleEntity rule)
        {
            var context = GetContext(rule);

            return context.Rule.IsEnabled && context.Rule.Trigger is not ManualTrigger;
        }

        public bool CanRunFromSnapshots(IRuleEntity rule)
        {
            var context = GetContext(rule);

            return CanRunRule(rule) && ruleService.CanCreateSnapshotEvents(context);
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

        private static RuleContext GetContext(IRuleEntity rule)
        {
            return new RuleContext
            {
                AppId = rule.AppId,
                Rule = rule.RuleDef,
                RuleId = rule.Id,
                IgnoreStale = false
            };
        }
    }
}
