// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Events;
using Squidex.Flows;
using Squidex.Flows.Internal;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules.Runner;

public sealed class DefaultRuleRunnerService(
    IJobService jobService,
    IEventFormatter eventFormatter,
    IEventStore eventStore,
    IFlowManager<FlowEventContext> flowManager,
    IRuleService ruleService)
    : IRuleRunnerService
{
    private const int MaxSimulatedEvents = 100;
    private static readonly RefToken SimulatorUser = RefToken.Client("Simulator");

    public IClock Clock { get; set; } = SystemClock.Instance;

    public Task<List<SimulatedRuleEvent>> SimulateAsync(NamedId<DomainId> appId, RuleTrigger trigger, FlowDefinition flow,
        CancellationToken ct = default)
    {
        var now = SystemClock.Instance.GetCurrentInstant();

        var rule = new Rule
        {
            AppId = appId,
            Created = now,
            CreatedBy = SimulatorUser,
            Flow = flow,
            IsEnabled = true,
            LastModified = now,
            LastModifiedBy = SimulatorUser,
            Trigger = trigger,
        };

        return SimulateAsync(rule, ct);
    }

    public async Task<List<SimulatedRuleEvent>> SimulateAsync(Rule rule,
        CancellationToken ct = default)
    {
        Guard.NotNull(rule);

        var context = new RulesContext
        {
            AppId = rule.AppId,
            IncludeSkipped = true,
            IncludeStale = true,
            Rules = new Dictionary<DomainId, Rule>
            {
                [rule.Id] = rule,
            }.ToReadonlyDictionary(),
        };

        var simulatedEvents = new List<SimulatedRuleEvent>(MaxSimulatedEvents);

        await foreach (var appEvent in QueryEventsAsync(rule, ct))
        {
            // Also create jobs for rules with failing conditions because we want to show them in the table.
            await foreach (var job in ruleService.CreateJobsAsync(appEvent, context, ct).Take(MaxSimulatedEvents).WithCancellation(ct))
            {
                var state =
                    job.Job != null ?
                    await flowManager.SimulateAsync(job.Job.Value, ct) :
                    null;

                var eventId = appEvent.Headers.EventId();

                simulatedEvents.Add(new SimulatedRuleEvent
                {
                    EnrichedEvent = job.EnrichedEvent,
                    Error = job.EnrichmentError?.Message,
                    Event = appEvent.Payload,
                    EventId = eventId,
                    EventName = ruleService.GetName(appEvent.Payload),
                    State = state,
                    SkipReason = job.SkipReason,
                    UniqueId = $"{eventId}_{job.Offset}",
                });
            }
        }

        return simulatedEvents;
    }

    private async IAsyncEnumerable<Envelope<AppEvent>> QueryEventsAsync(Rule rule,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var appId = rule.AppId;

        if (rule.Trigger is ManualTrigger)
        {
            yield return Envelope.Create<AppEvent>(
                new RuleManuallyTriggered
                {
                    Actor = SimulatorUser,
                    AppId = appId,
                    RuleId = rule.Id,
                });

            yield break;
        }

        if (rule.Trigger is CronJobTrigger cronJob)
        {
            yield return Envelope.Create<AppEvent>(
                new RuleCronJobTriggered
                {
                    Actor = SimulatorUser,
                    AppId = appId,
                    RuleId = rule.Id,
                    Value = cronJob.Value,
                });

            yield break;
        }

        var streamStart = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(7)).ToDateTimeUtc();
        var streamFilter = StreamFilter.Prefix($"%-{appId.Id}");

        await foreach (var storedEvent in eventStore.QueryAllReverseAsync(streamFilter, streamStart, MaxSimulatedEvents, ct))
        {
            var @event = eventFormatter.ParseIfKnown(storedEvent);

            if (@event?.Payload is AppEvent)
            {
                yield return @event.To<AppEvent>();
            }
        }
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
