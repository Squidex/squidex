// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Tasks;

#pragma warning disable SA1401 // Fields should be private

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed class RuleService : IRuleService
{
    private readonly Dictionary<Type, IRuleActionHandler> ruleActionHandlers;
    private readonly Dictionary<Type, IRuleTriggerHandler> ruleTriggerHandlers;
    private readonly TypeRegistry typeRegistry;
    private readonly RuleOptions ruleOptions;
    private readonly IEventEnricher eventEnricher;
    private readonly IJsonSerializer serializer;
    private readonly ILogger<RuleService> log;

    private sealed class RuleState
    {
        public SkipReason Skip { get; set; }

        public Rule Rule { get; init; }

        public IRuleActionHandler ActionHandler;
    }

    public IClock Clock { get; set; } = SystemClock.Instance;

    public RuleService(
        IOptions<RuleOptions> ruleOptions,
        IEnumerable<IRuleTriggerHandler> ruleTriggerHandlers,
        IEnumerable<IRuleActionHandler> ruleActionHandlers,
        IEventEnricher eventEnricher,
        IJsonSerializer serializer,
        ILogger<RuleService> log,
        TypeRegistry typeRegistry)
    {
        this.typeRegistry = typeRegistry;
        this.eventEnricher = eventEnricher;
        this.ruleOptions = ruleOptions.Value;
        this.ruleTriggerHandlers = ruleTriggerHandlers.ToDictionary(x => x.TriggerType);
        this.ruleActionHandlers = ruleActionHandlers.ToDictionary(x => x.ActionType);
        this.serializer = serializer;
        this.log = log;
    }

    public bool CanCreateSnapshotEvents(Rule rule)
    {
        if (!ruleTriggerHandlers.TryGetValue(rule.Trigger.GetType(), out var triggerHandler))
        {
            return false;
        }

        return triggerHandler.CanCreateSnapshotEvents;
    }

    public async IAsyncEnumerable<JobResult> CreateSnapshotJobsAsync(RuleContext context,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var rule = context.Rule;

        if (!rule.IsEnabled && !context.IncludeSkipped)
        {
            yield break;
        }

        if (!ruleTriggerHandlers.TryGetValue(rule.Trigger.GetType(), out var triggerHandler))
        {
            yield break;
        }

        if (!ruleActionHandlers.TryGetValue(rule.Action.GetType(), out var actionHandler))
        {
            yield break;
        }

        if (!triggerHandler.CanCreateSnapshotEvents)
        {
            yield break;
        }

        var now = Clock.GetCurrentInstant();

        // Maintain an offset per event to generate a unique ID.
        var offset = 0;

        await foreach (var enrichedEvent in triggerHandler.CreateSnapshotEventsAsync(context, ct))
        {
            JobResult? job;
            try
            {
                await eventEnricher.EnrichAsync(enrichedEvent, null);

                if (!triggerHandler.Trigger(enrichedEvent, context.Rule.Trigger))
                {
                    continue;
                }

                job = await CreateJobAsync(enrichedEvent, actionHandler, context.Rule, now);
                job.Offset = offset++;
            }
            catch (Exception ex)
            {
                job = JobResult.Failed(ex);
            }

            yield return job;
        }
    }

    public IAsyncEnumerable<JobResult> CreateJobsAsync(Envelope<IEvent> @event, RulesContext context,
        CancellationToken ct = default)
    {
        Guard.NotNull(@event);

        // Each rule can has its own errors.
        var states = context.Rules.Select(x => new RuleState { Rule = x.Value }).ToList();

        var allResults =
            CreateJobs(@event, context, states, ct)
                .Catch(ex =>
                {
                    log.LogError(ex, "Failed to create rule job.");

                    return states.Select(state => JobResult.Skipped(state.Rule, SkipReason.Failed));
                });

        return allResults;
    }

    private async IAsyncEnumerable<JobResult> CreateJobs(Envelope<IEvent> @event, RulesContext context, List<RuleState> states,
        [EnumeratorCancellation] CancellationToken ct)
    {
        if (@event.Payload is not AppEvent)
        {
            foreach (var state in states)
            {
                yield return JobResult.Skipped(state.Rule, SkipReason.WrongEvent);
            }

            yield break;
        }

        var typed = @event.To<AppEvent>();

        if (typed.Payload.FromRule)
        {
            if (context.IncludeSkipped)
            {
                foreach (var state in states)
                {
                    state.Skip |= SkipReason.FromRule;
                }
            }
            else
            {
                foreach (var state in states)
                {
                    yield return JobResult.Skipped(state.Rule, SkipReason.FromRule);
                }

                yield break;
            }
        }

        var now = Clock.GetCurrentInstant();

        var eventTime =
            @event.Headers.ContainsKey(CommonHeaders.Timestamp) ?
            @event.Headers.Timestamp() :
            now;

        if (!context.IncludeStale && eventTime.Plus(Constants.StaleTime) < now)
        {
            if (context.IncludeSkipped)
            {
                foreach (var state in states)
                {
                    state.Skip |= SkipReason.TooOld;
                }
            }
            else
            {
                foreach (var state in states)
                {
                    yield return JobResult.Skipped(state.Rule, SkipReason.TooOld);
                }

                yield break;
            }
        }

        Dictionary<IRuleTriggerHandler, List<RuleState>>? matchingRules = null;

        foreach (var state in states)
        {
            var rule = state.Rule;

            if (!rule.IsEnabled)
            {
                if (context.IncludeSkipped)
                {
                    state.Skip = SkipReason.Disabled;
                }
                else
                {
                    yield return JobResult.Skipped(state.Rule, SkipReason.Disabled);
                    continue;
                }
            }

            var actionType = rule.Action.GetType();

            if (!ruleTriggerHandlers.TryGetValue(rule.Trigger.GetType(), out var triggerHandler))
            {
                yield return JobResult.Skipped(state.Rule, SkipReason.NoTrigger);
                continue;
            }

            if (!triggerHandler.Handles(typed.Payload))
            {
                yield return JobResult.Skipped(state.Rule, SkipReason.WrongEventForTrigger);
                continue;
            }

            if (!ruleActionHandlers.TryGetValue(actionType, out state.ActionHandler!))
            {
                yield return JobResult.Skipped(state.Rule, SkipReason.NoAction);
                continue;
            }

            if (!triggerHandler.Trigger(typed, rule.Trigger))
            {
                if (context.IncludeSkipped)
                {
                    state.Skip = SkipReason.ConditionPrecheckDoesNotMatch;
                }
                else
                {
                    yield return JobResult.Skipped(state.Rule, SkipReason.ConditionPrecheckDoesNotMatch);
                    continue;
                }
            }

            matchingRules ??= [];
            matchingRules.GetOrAddNew(triggerHandler).Add(state);
        }

        if (matchingRules == null)
        {
            yield break;
        }

        foreach (var (triggerHandler, rulesByTrigger) in matchingRules)
        {
            var triggerResults =
                CreateTriggerJobs(typed, triggerHandler, rulesByTrigger, now, context, ct)
                    .Catch(ex =>
                    {
                        log.LogError(ex, "Failed to create rule jobs from trigger.");

                        return states.Select(state => JobResult.Skipped(state.Rule, SkipReason.Failed));
                    });

            await foreach (var result in triggerResults.WithCancellation(ct))
            {
                yield return result;
            }
        }
    }

    private async IAsyncEnumerable<JobResult> CreateTriggerJobs(Envelope<AppEvent> @event, IRuleTriggerHandler triggerHandler, List<RuleState> states, Instant now, RulesContext context,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var takeEvents = context.MaxEvents ?? int.MaxValue;

        await foreach (var enrichedEvent in triggerHandler.CreateEnrichedEventsAsync(@event, context, ct).Take(takeEvents).WithCancellation(ct))
        {
            // Maintain an offset per event to generate a unique ID.
            var offset = 0;

            var eventResults =
                CreateEventJobs(@event, enrichedEvent, triggerHandler, states, now, context)
                    .Catch(ex =>
                    {
                        log.LogError(ex, "Failed to create rule jobs from event.");

                        return states.Select(state =>
                            new JobResult
                            {
                                EnrichedEvent = enrichedEvent,
                                EnrichmentError = ex,
                                Rule = state.Rule,
                                SkipReason = SkipReason.Failed,
                            });
                    });

            await foreach (var result in eventResults.WithCancellation(ct))
            {
                result.Offset = offset++;

                yield return result;
            }
        }
    }

    private async IAsyncEnumerable<JobResult> CreateEventJobs(Envelope<AppEvent> @event, EnrichedEvent enrichedEvent, IRuleTriggerHandler triggerHandler, List<RuleState> states, Instant now, RulesContext context)
    {
        if (string.IsNullOrWhiteSpace(enrichedEvent.Name))
        {
            enrichedEvent.Name = GetName(@event.Payload);
        }

        await eventEnricher.EnrichAsync(enrichedEvent, @event);

        foreach (var state in states)
        {
            // The actual skip reason could be different per event.
            var skipped = state.Skip;

            if (!triggerHandler.Trigger(enrichedEvent, state.Rule.Trigger))
            {
                if (context.IncludeSkipped)
                {
                    skipped |= SkipReason.ConditionDoesNotMatch;
                }
                else
                {
                    yield return new JobResult
                    {
                        EnrichedEvent = enrichedEvent,
                        EnrichmentError = null,
                        Rule = state.Rule,
                        SkipReason = SkipReason.ConditionDoesNotMatch
                    };

                    continue;
                }
            }

            var result = await CreateJobAsync(enrichedEvent, state.ActionHandler, state.Rule, now);

            // If the conditions matchs, we can skip creating a new object and save a few allocations.
            if (skipped != SkipReason.None)
            {
                result = result with { SkipReason = skipped };
            }

            yield return result;
        }
    }

    private async Task<JobResult> CreateJobAsync(EnrichedEvent enrichedEvent, IRuleActionHandler actionHandler, Rule rule, Instant now)
    {
        var actionType = rule.Action.GetType();
        var actionName = typeRegistry.GetName<RuleAction>(actionType);

        var expires = now.Plus(Constants.ExpirationTime);

        var job = new RuleJob
        {
            Id = DomainId.NewGuid(),
            ActionData = string.Empty,
            ActionName = actionName,
            AppId = enrichedEvent.AppId.Id,
            Created = now,
            EventName = enrichedEvent.Name,
            ExecutionPartition = enrichedEvent.Partition,
            Expires = expires,
            RuleId = rule.Id
        };

        try
        {
            var (description, data) = await actionHandler.CreateJobAsync(enrichedEvent, rule.Action);

            var json = serializer.Serialize(data);

            job.ActionData = json;
            job.ActionName = actionName;
            job.Description = description;

            return new JobResult
            {
                EnrichedEvent = enrichedEvent,
                EnrichmentError = null,
                Rule = rule,
                Job = job,
            };
        }
        catch (Exception ex)
        {
            job.Description = "Failed to create job";

            return JobResult.Failed(ex, enrichedEvent, job);
        }
    }

    public string GetName(AppEvent @event)
    {
        foreach (var (_, handler) in ruleTriggerHandlers)
        {
            if (handler.Handles(@event))
            {
                var name = handler.GetName(@event);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }
            }
        }

        return @event.GetType().Name;
    }

    public async Task<(Result Result, TimeSpan Elapsed)> InvokeAsync(string actionName, string job,
        CancellationToken ct = default)
    {
        var actionWatch = ValueStopwatch.StartNew();

        Result result;
        try
        {
            var actionType = typeRegistry.GetType<RuleAction>(actionName);
            var actionHandler = ruleActionHandlers[actionType];
            var actionObject = serializer.Deserialize<object>(job, actionHandler.DataType);

            using (var combined = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                // Enforce a timeout after a configured time span.
                combined.CancelAfter(GetTimeoutInMs());

                result = await actionHandler.ExecuteJobAsync(actionObject, combined.Token).WithCancellation(combined.Token);
            }
        }
        catch (Exception ex)
        {
            result = Result.Failed(ex);
        }

        var elapsed = TimeSpan.FromMilliseconds(actionWatch.Stop());

        result.Enrich(elapsed);

        return (result, elapsed);
    }

    private int GetTimeoutInMs()
    {
        return ruleOptions.ExecutionTimeoutInSeconds * 1000;
    }
}
