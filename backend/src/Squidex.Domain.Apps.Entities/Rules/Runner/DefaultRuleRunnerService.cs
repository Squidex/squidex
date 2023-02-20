// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Rules.Runner;

public sealed class DefaultRuleRunnerService : IRuleRunnerService
{
    private const int MaxSimulatedEvents = 100;
    private readonly IPersistenceFactory<RuleRunnerState> persistenceFactory;
    private readonly IEventFormatter eventFormatter;
    private readonly IEventStore eventStore;
    private readonly IRuleService ruleService;
    private readonly IMessageBus messaging;

    public DefaultRuleRunnerService(
        IPersistenceFactory<RuleRunnerState> persistenceFactory,
        IEventFormatter eventFormatter,
        IEventStore eventStore,
        IRuleService ruleService,
        IMessageBus messaging)
    {
        this.eventFormatter = eventFormatter;
        this.persistenceFactory = persistenceFactory;
        this.eventStore = eventStore;
        this.ruleService = ruleService;
        this.messaging = messaging;
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

        var context = new RulesContext
        {
            AppId = appId,
            IncludeSkipped = true,
            IncludeStale = true,
            Rules = new Dictionary<DomainId, Rule>
            {
                [ruleId] = rule
            }.ToReadonlyDictionary()
        };

        var simulatedEvents = new List<SimulatedRuleEvent>(MaxSimulatedEvents);

        var fromNow = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(7));

        await foreach (var storedEvent in eventStore.QueryAllReverseAsync($"^([a-zA-Z0-9]+)\\-{appId.Id}", fromNow, MaxSimulatedEvents, ct))
        {
            var @event = eventFormatter.ParseIfKnown(storedEvent);

            if (@event?.Payload is not AppEvent appEvent)
            {
                continue;
            }

            // Also create jobs for rules with failing conditions because we want to show them in th table.
            await foreach (var job in ruleService.CreateJobsAsync(@event, context, ct).Take(MaxSimulatedEvents).WithCancellation(ct))
            {
                var eventName = job.Job?.EventName;

                if (string.IsNullOrWhiteSpace(eventName))
                {
                    eventName = ruleService.GetName(appEvent);
                }

                var eventId = @event.Headers.EventId();

                simulatedEvents.Add(new SimulatedRuleEvent
                {
                    ActionData = job.Job?.ActionData,
                    ActionName = job.Job?.ActionName,
                    EnrichedEvent = job.EnrichedEvent,
                    Error = job.EnrichmentError?.Message,
                    Event = @event.Payload,
                    EventId = eventId,
                    EventName = eventName,
                    SkipReason = job.SkipReason,
                    UniqueId = $"{eventId}_{job.Offset}",
                });
            }
        }

        return simulatedEvents;
    }

    public bool CanRunRule(IRuleEntity rule)
    {
        return rule.RuleDef.Trigger is not ManualTrigger;
    }

    public bool CanRunFromSnapshots(IRuleEntity rule)
    {
        return rule.RuleDef.Trigger is not ManualTrigger && ruleService.CanCreateSnapshotEvents(rule.RuleDef);
    }

    public Task CancelAsync(DomainId appId,
        CancellationToken ct = default)
    {
        return messaging.PublishAsync(new RuleRunnerCancel(appId), ct: ct);
    }

    public Task RunAsync(DomainId appId, DomainId ruleId, bool fromSnapshots = false,
        CancellationToken ct = default)
    {
        return messaging.PublishAsync(new RuleRunnerRun(appId, ruleId, fromSnapshots), ct: ct);
    }

    public async Task<DomainId?> GetRunningRuleIdAsync(DomainId appId,
        CancellationToken ct = default)
    {
        var state = await GetStateAsync(appId, ct);

        return state.Value.RuleId;
    }

    private async Task<SimpleState<RuleRunnerState>> GetStateAsync(DomainId appId,
        CancellationToken ct)
    {
        var state = new SimpleState<RuleRunnerState>(persistenceFactory, GetType(), appId);

        await state.LoadAsync(ct);

        return state;
    }
}
