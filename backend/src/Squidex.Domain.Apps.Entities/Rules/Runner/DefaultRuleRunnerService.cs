// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Orleans;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules.Runner
{
    public sealed class DefaultRuleRunnerService : IRuleRunnerService
    {
        private const int MaxSimulatedEvents = 100;
        private readonly IEventFormatter eventFormatter;
        private readonly IEventStore eventStore;
        private readonly IGrainFactory grainFactory;
        private readonly IRuleService ruleService;

        public DefaultRuleRunnerService(IGrainFactory grainFactory,
            IEventFormatter eventFormatter,
            IEventStore eventStore,
            IRuleService ruleService)
        {
            this.grainFactory = grainFactory;
            this.eventFormatter = eventFormatter;
            this.eventStore = eventStore;
            this.ruleService = ruleService;
        }

        public Task<List<SimulatedRuleEvent>> SimulateAsync(IRuleEntity rule,
            CancellationToken ct = default)
        {
            return SimulateAsync(rule.AppId, rule.Id, rule.RuleDef, ct);
        }

        public async Task<List<SimulatedRuleEvent>> SimulateAsync(NamedId<DomainId> appId, DomainId ruleId, Rule rule,
            CancellationToken ct = default)
        {
            Guard.NotNull(rule);

            var context = new RuleContext
            {
                AppId = appId,
                Rule = rule,
                RuleId = ruleId,
                IncludeSkipped = true,
                IncludeStale = true
            };

            var simulatedEvents = new List<SimulatedRuleEvent>(MaxSimulatedEvents);

            var fromNow = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(7));

            await foreach (var storedEvent in eventStore.QueryAllReverseAsync($"^([a-zA-Z0-9]+)\\-{appId.Id}", fromNow, MaxSimulatedEvents, ct))
            {
                var @event = eventFormatter.ParseIfKnown(storedEvent);

                if (@event?.Payload is AppEvent appEvent)
                {
                    // Also create jobs for rules with failing conditions because we want to show them in th table.
                    await foreach (var result in ruleService.CreateJobsAsync(@event, context, ct))
                    {
                        var eventName = result.Job?.EventName;

                        if (string.IsNullOrWhiteSpace(eventName))
                        {
                            eventName = ruleService.GetName(appEvent);
                        }

                        simulatedEvents.Add(new SimulatedRuleEvent
                        {
                            ActionData = result.Job?.ActionData,
                            ActionName = result.Job?.ActionName,
                            EnrichedEvent = result.EnrichedEvent,
                            Error = result.EnrichmentError?.Message,
                            Event = @event.Payload,
                            EventId = @event.Headers.EventId(),
                            EventName = eventName,
                            SkipReason = result.SkipReason
                        });
                    }
                }
            }

            return simulatedEvents;
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

        public Task CancelAsync(DomainId appId,
            CancellationToken ct = default)
        {
            var grain = grainFactory.GetGrain<IRuleRunnerGrain>(appId.ToString());

            return grain.CancelAsync();
        }

        public Task<DomainId?> GetRunningRuleIdAsync(DomainId appId,
            CancellationToken ct = default)
        {
            var grain = grainFactory.GetGrain<IRuleRunnerGrain>(appId.ToString());

            return grain.GetRunningRuleIdAsync();
        }

        public Task RunAsync(DomainId appId, DomainId ruleId, bool fromSnapshots = false,
            CancellationToken ct = default)
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
                RuleId = rule.Id
            };
        }
    }
}
