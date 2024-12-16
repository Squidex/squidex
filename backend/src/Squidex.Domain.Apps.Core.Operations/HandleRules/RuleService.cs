// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using NodaTime;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Events;
using Squidex.Flows.Execution;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed partial class RuleService(
    IFlowExecutor<RuleFlowContext> flowExecutor,
    IEnumerable<IRuleTriggerHandler> ruleTriggers,
    IEventEnricher eventEnricher,
    ILogger<RuleService> log)
    : IRuleService
{
    private readonly Dictionary<Type, IRuleTriggerHandler> ruleTriggerHandlers = ruleTriggers.ToDictionary(x => x.TriggerType);

    public IClock Clock { get; set; } = SystemClock.Instance;

    public bool CanCreateSnapshotEvents(Rule rule)
    {
        if (!ruleTriggerHandlers.TryGetValue(rule.Trigger.GetType(), out var handler))
        {
            return false;
        }

        return handler.CanCreateSnapshotEvents;
    }

    public async IAsyncEnumerable<JobResult> CreateSnapshotJobsAsync(RuleContext context,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var rule = context.Rule;

        if (!rule.IsEnabled && !context.IncludeSkipped)
        {
            yield break;
        }

        if (!ruleTriggerHandlers.TryGetValue(rule.Trigger.GetType(), out var handler) || !handler.CanCreateSnapshotEvents)
        {
            yield break;
        }

        if (!handler.CanCreateSnapshotEvents)
        {
            yield break;
        }

        await foreach (var enrichedEvent in handler.CreateSnapshotEventsAsync(context, ct))
        {
            JobResult? job;
            try
            {
                await eventEnricher.EnrichAsync(enrichedEvent, null);

                if (!handler.Trigger(enrichedEvent, rule.Trigger))
                {
                    continue;
                }

                job = await CreateJobAsync(enrichedEvent, context.Rule, ct);
            }
            catch (Exception ex)
            {
                job = JobResult.Failed("None", SkipReason.Failed, rule, exception: ex);
            }

            yield return job;
        }
    }

    public IAsyncEnumerable<JobResult> CreateJobsAsync(Envelope<AppEvent> @event, RulesContext context,
        CancellationToken ct = default)
    {
        Guard.NotNull(@event);

        // Each rule can has its own errors.
        var states = new RuleStates(context.Rules.Select(x => new RuleState(x.Value)));

        // Ensure that we calculate the event name only once.
        var eventName = GetName(@event.Payload);

        var allResults =
            CreateJobsAsync(eventName, @event, states, context, ct)
                .Catch(ex =>
                {
                    log.LogError(ex, "Failed to create rule job.");

                    return states.Select(s =>
                        JobResult.Failed(eventName, SkipReason.Failed, s.Rule, exception: ex)
                    );
                });

        return allResults;
    }

    private async IAsyncEnumerable<JobResult> CreateJobsAsync(
        string eventName,
        Envelope<AppEvent> @event,
        RuleStates states,
        RulesContext context,
        [EnumeratorCancellation] CancellationToken ct)
    {
        if (@event.Payload.FromRule)
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
                    yield return JobResult.Failed(eventName, SkipReason.FromRule, state.Rule);
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
                    yield return JobResult.Failed(eventName, SkipReason.FromRule, state.Rule);
                }

                yield break;
            }
        }

        Dictionary<IRuleTriggerHandler, RuleStates>? matchingRules = null;
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
                    yield return JobResult.Failed(eventName, SkipReason.FromRule, state.Rule);
                }
            }

            if (!ruleTriggerHandlers.TryGetValue(rule.Trigger.GetType(), out var triggerHandler))
            {
                yield return JobResult.Failed(eventName, SkipReason.NoTrigger, state.Rule);
                continue;
            }

            if (!triggerHandler.Handles(@event.Payload))
            {
                yield return JobResult.Failed(eventName, SkipReason.WrongEventForTrigger, state.Rule);
                continue;
            }

            if (!triggerHandler.Trigger(@event, rule.Trigger))
            {
                if (context.IncludeSkipped)
                {
                    state.Skip = SkipReason.ConditionPrecheckDoesNotMatch;
                }
                else
                {
                    yield return JobResult.Failed(eventName, SkipReason.ConditionPrecheckDoesNotMatch, state.Rule);
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
                CreateTriggerJobsAsync(eventName, @event, triggerHandler, rulesByTrigger, context, ct)
                    .Catch(ex =>
                    {
                        log.LogError(ex, "Failed to create rule jobs from trigger.");

                        return states.Select(s =>
                            JobResult.Failed(eventName, SkipReason.Failed, s.Rule, exception: ex)
                        );
                    });

            await foreach (var result in triggerResults.WithCancellation(ct))
            {
                yield return result;
            }
        }
    }

    private async IAsyncEnumerable<JobResult> CreateTriggerJobsAsync(
        string eventName,
        Envelope<AppEvent> @event,
        IRuleTriggerHandler handler,
        RuleStates states,
        RulesContext context,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var enrichedEvent in handler.CreateEnrichedEventsAsync(@event, context, ct).Take(context.MaxEvents).WithCancellation(ct))
        {
            if (string.IsNullOrWhiteSpace(enrichedEvent.Name))
            {
                enrichedEvent.Name = eventName;
            }

            var eventResults =
                CreateEventJobsAsync(@event, enrichedEvent, handler, states, context, ct)
                    .Catch(ex =>
                    {
                        log.LogError(ex, "Failed to create rule jobs from event.");

                        return states.Select(s =>
                            JobResult.Failed(eventName, SkipReason.Failed, s.Rule, enrichedEvent, ex)
                        );
                    });

            await foreach (var result in eventResults.WithCancellation(ct))
            {
                yield return result;
            }
        }
    }

    private async IAsyncEnumerable<JobResult> CreateEventJobsAsync(
        Envelope<AppEvent> @event,
        EnrichedEvent enrichedEvent,
        IRuleTriggerHandler handler,
        RuleStates states,
        RulesContext context,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await eventEnricher.EnrichAsync(enrichedEvent, @event);

        foreach (var state in states)
        {
            // The actual skip reason could be different per event.
            var skipped = state.Skip;

            if (!handler.Trigger(enrichedEvent, state.Rule.Trigger))
            {
                if (context.IncludeSkipped)
                {
                    skipped |= SkipReason.ConditionDoesNotMatch;
                }
                else
                {
                    yield return new JobResult
                    {
                        Rule = state.Rule,
                        EnrichedEvent = enrichedEvent,
                        EnrichmentError = null,
                        EventName = GetName(@event.Payload),
                        SkipReason = SkipReason.ConditionDoesNotMatch
                    };

                    continue;
                }
            }

            var result = await CreateJobAsync(enrichedEvent, state.Rule, ct);

            // If the conditions matchs, we can skip creating a new object and save a few allocations.
            if (skipped != SkipReason.None)
            {
                result = result with { SkipReason = skipped };
            }

            yield return result;
        }
    }

    private async Task<JobResult> CreateJobAsync(EnrichedEvent enrichedEvent, Rule rule,
        CancellationToken ct)
    {
        try
        {
            var context = new RuleFlowContext
            {
                Event = enrichedEvent,
            };

            var state =
                await flowExecutor.CreateInstanceAsync(
                    enrichedEvent.AppId.Id.ToString(),
                    rule.Id.ToString(),
                    enrichedEvent.Name,
                    rule.Flow,
                    context,
                    ct);

            return new JobResult
            {
                Rule = rule,
                EnrichedEvent = enrichedEvent,
                EnrichmentError = null,
                EventName = enrichedEvent.Name,
                SkipReason = default,
                State = state,
            };
        }
        catch (Exception ex)
        {
            return JobResult.Failed(enrichedEvent.Name, SkipReason.Failed, rule, enrichedEvent, ex);
        }
    }
}
