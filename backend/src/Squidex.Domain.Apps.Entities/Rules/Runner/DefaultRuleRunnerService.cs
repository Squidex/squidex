// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules.Runner;

public sealed class DefaultRuleRunnerService : IRuleRunnerService
{
    private const int MaxSimulatedEvents = 100;
    private readonly IJobService jobService;
    private readonly IEventFormatter eventFormatter;
    private readonly IEventStore eventStore;
    private readonly IRuleService ruleService;

    public DefaultRuleRunnerService(
        IJobService jobService,
        IEventFormatter eventFormatter,
        IEventStore eventStore,
        IRuleService ruleService)
    {
        this.jobService = jobService;
        this.eventFormatter = eventFormatter;
        this.eventStore = eventStore;
        this.ruleService = ruleService;
    }

    public Task<List<SimulatedRuleEvent>> SimulateAsync(Rule rule,
        CancellationToken ct = default)
    {
        return SimulateAsync(rule.AppId, rule.Id, rule, ct);
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

        var streamStart = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(7));
        var streamFilter = StreamFilter.Prefix($"([a-zA-Z0-9]+)-{appId.Id}");

        await foreach (var storedEvent in eventStore.QueryAllReverseAsync(streamFilter, streamStart, MaxSimulatedEvents, ct))
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

    public bool CanRunRule(Rule rule)
    {
        return rule.Trigger is not ManualTrigger;
    }

    public bool CanRunFromSnapshots(Rule rule)
    {
        return rule.Trigger is not ManualTrigger && ruleService.CanCreateSnapshotEvents(rule);
    }

    public Task CancelAsync(DomainId appId,
        CancellationToken ct = default)
    {
        var taskName = RuleRunnerJob.TaskName;

        return jobService.CancelAsync(appId, taskName, ct);
    }

    public Task RunAsync(RefToken actor, App app, DomainId ruleId, bool fromSnapshots = false,
        CancellationToken ct = default)
    {
        var job = RuleRunnerJob.BuildRequest(actor, app, ruleId, fromSnapshots);

        return jobService.StartAsync(app.Id, job, ct);
    }

    public async Task<DomainId?> GetRunningRuleIdAsync(DomainId appId,
        CancellationToken ct = default)
    {
        var jobs = await jobService.GetJobsAsync(appId, ct);

        return jobs.Select(RuleRunnerJob.GetRunningRuleId).FirstOrDefault(x => x != null);
    }
}
