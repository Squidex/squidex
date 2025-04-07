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
using NodaTime.Extensions;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Events;
using Squidex.Flows.Internal.Execution;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed class RuleService(
    IEnumerable<IRuleTriggerHandler> ruleTriggerHandlers,
    IEventEnricher eventEnricher,
    IOptions<RulesOptions> options,
    ILogger<RuleService> log)
    : IRuleService
{
    private readonly Dictionary<Type, IRuleTriggerHandler> ruleTriggerHandlers = ruleTriggerHandlers.ToDictionary(x => x.TriggerType);
    private readonly Duration staleTime = options.Value.StaleTime.ToDuration();

    private sealed class RuleState
    {
        public SkipReason Skip { get; set; }

        public Rule Rule { get; init; }
    }

    public IClock Clock { get; set; } = SystemClock.Instance;

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

        if (!triggerHandler.CanCreateSnapshotEvents)
        {
            yield break;
        }

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

                job = CreateJob(enrichedEvent, context.Rule);
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
            @event.Headers.TimestampAsInstant() :
            now;

        if (!context.IncludeStale && eventTime.Plus(staleTime) < now)
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
                CreateTriggerJobs(typed, triggerHandler, rulesByTrigger, context, ct)
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

    private async IAsyncEnumerable<JobResult> CreateTriggerJobs(Envelope<AppEvent> @event, IRuleTriggerHandler triggerHandler, List<RuleState> states, RulesContext context,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var takeEvents = context.MaxEvents ?? int.MaxValue;

        await foreach (var enrichedEvent in triggerHandler.CreateEnrichedEventsAsync(@event, context, ct).Take(takeEvents).WithCancellation(ct))
        {
            // Maintain an offset per event to generate a unique ID.
            var offset = 0;

            var eventResults =
                CreateEventJobs(@event, enrichedEvent, triggerHandler, states, context)
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

    private async IAsyncEnumerable<JobResult> CreateEventJobs(Envelope<AppEvent> @event, EnrichedEvent enrichedEvent, IRuleTriggerHandler triggerHandler, List<RuleState> states, RulesContext context)
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
                        SkipReason = SkipReason.ConditionDoesNotMatch,
                    };

                    continue;
                }
            }

            var result = CreateJob(enrichedEvent, state.Rule);

            // If the conditions matchs, we can skip creating a new object and save a few allocations.
            if (skipped != SkipReason.None)
            {
                result = result with { SkipReason = skipped };
            }

            yield return result;
        }
    }

    private static JobResult CreateJob(EnrichedEvent enrichedEvent, Rule rule)
    {
        var job = new CreateFlowInstanceRequest<FlowEventContext>
        {
            OwnerId = enrichedEvent.AppId.Id.ToString(),
            DefinitionId = rule.Id.ToString(),
            Definition = rule.Flow,
            Description = string.Empty,
            Context = new FlowEventContext { Event = enrichedEvent },
        };

        return new JobResult
        {
            Job = job,
            EnrichedEvent = enrichedEvent,
            EnrichmentError = null,
            Rule = rule,
        };
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
}
