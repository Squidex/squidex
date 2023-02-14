// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

        public bool JobCreated { get; set; }

        public Rule Rule { get; init; }

        public DomainId RuleId { get; init; }
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

    public async Task CreateSnapshotJobsAsync(JobCallback callback, RuleContext context,
        CancellationToken ct = default)
    {
        Guard.NotNull(callback);

        var rule = context.Rule;

        if (!rule.IsEnabled && !context.IncludeSkipped)
        {
            return;
        }

        if (!ruleTriggerHandlers.TryGetValue(rule.Trigger.GetType(), out var triggerHandler))
        {
            return;
        }

        if (!ruleActionHandlers.TryGetValue(rule.Action.GetType(), out var actionHandler))
        {
            return;
        }

        if (!triggerHandler.CanCreateSnapshotEvents)
        {
            return;
        }

        var now = Clock.GetCurrentInstant();

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

                job = await CreateJobAsync(actionHandler, enrichedEvent, context.RuleId, context.Rule, now);
            }
            catch (Exception ex)
            {
                job = JobResult.Failed(ex);
            }

            await callback(context.RuleId, context.Rule, job, ct);
        }
    }

    public async Task CreateJobsAsync(JobCallback callback, Envelope<IEvent> @event, RulesContext context,
        CancellationToken ct = default)
    {
        Guard.NotNull(callback);
        Guard.NotNull(@event);

        // Each rule should create exactly one skip reason.
        var ruleStates = new Dictionary<DomainId, RuleState>();

        foreach (var (ruleId, rule) in context.Rules)
        {
            ruleStates[ruleId] = new RuleState { Rule = rule };
        }

        ValueTask WriteAsync(RuleState ruleState, JobResult result)
        {
            ruleState.JobCreated = true;

            return callback(ruleState.RuleId, ruleState.Rule, result, ct);
        }

        async ValueTask<bool> SkipAll(SkipReason skip, JobResult result, bool force = false)
        {
            // For the simulation we want to proceed as much as possible.
            if (context.IncludeSkipped && !force)
            {
                foreach (var (_, ruleState) in ruleStates)
                {
                    ruleState.Skip |= skip;
                }

                return true;
            }
            else
            {
                foreach (var (_, ruleState) in ruleStates)
                {
                    await WriteAsync(ruleState, result);
                }

                return false;
            }
        }

        async ValueTask<bool> SkipOne(RuleState ruleState, SkipReason skip, JobResult result, bool force = false)
        {
            // For the simulation we want to proceed as much as possible.
            if (context.IncludeSkipped && !force)
            {
                ruleState.Skip |= skip;
                return true;
            }
            else
            {
                await WriteAsync(ruleState, result);
                return false;
            }
        }

        try
        {
            if (@event.Payload is not AppEvent)
            {
                await SkipAll(SkipReason.FromRule, JobResult.FromRule, true);
                return;
            }

            var typed = @event.To<AppEvent>();

            if (typed.Payload.FromRule)
            {
                if (!await SkipAll(SkipReason.FromRule, JobResult.FromRule))
                {
                    return;
                }
            }

            var now = Clock.GetCurrentInstant();

            var eventTime =
                @event.Headers.ContainsKey(CommonHeaders.Timestamp) ?
                @event.Headers.Timestamp() :
                now;

            if (!context.IncludeStale && eventTime.Plus(Constants.StaleTime) < now)
            {
                if (!await SkipAll(SkipReason.TooOld, JobResult.TooOld))
                {
                    return;
                }
            }

            Dictionary<IRuleTriggerHandler, List<(DomainId, Rules.Rule, IRuleActionHandler)>>? matchingRules = null;

            foreach (var (ruleId, rule) in context.Rules)
            {
                if (!rule.IsEnabled)
                {
                    // For the simulation we want to proceed as much as possible.
                    if (context.IncludeSkipped)
                    {
                        skipReason |= SkipReason.Disabled;
                    }
                    else
                    {
                        await WriteAsync(ruleId, rule, JobResult.Disabled);
                        break;
                    }
                }

                var actionType = rule.Action.GetType();

                if (!ruleTriggerHandlers.TryGetValue(rule.Trigger.GetType(), out var triggerHandler))
                {
                    await WriteAsync(ruleId, rule, JobResult.NoTrigger);
                    break;
                }

                if (!triggerHandler.Handles(typed.Payload))
                {
                    await WriteAsync(ruleId, rule, JobResult.WrongEventForTrigger);
                    break;
                }

                if (!ruleActionHandlers.TryGetValue(actionType, out var actionHandler))
                {
                    await WriteAsync(ruleId, rule, JobResult.NoAction);
                    break;
                }

                if (!triggerHandler.Trigger(typed, rule.Trigger))
                {
                    // For the simulation we want to proceed as much as possible.
                    if (context.IncludeSkipped)
                    {
                        skipReason |= SkipReason.ConditionPrecheckDoesNotMatch;
                    }
                    else
                    {
                        await WriteAsync(ruleId, rule, JobResult.ConditionPrecheckDoesNotMatch);
                        break;
                    }
                }

                matchingRules ??= new Dictionary<IRuleTriggerHandler, List<(DomainId, Rules.Rule, IRuleActionHandler)>>();
                matchingRules.GetOrAddNew(triggerHandler).Add((ruleId, rule, actionHandler));
            }

            if (matchingRules == null)
            {
                return;
            }

            foreach (var (triggerHandler, rules) in matchingRules)
            {
                try
                {
                    await foreach (var enrichedEvent in triggerHandler.CreateEnrichedEventsAsync(typed, context, ct))
                    {
                        if (string.IsNullOrWhiteSpace(enrichedEvent.Name))
                        {
                            enrichedEvent.Name = GetName(typed.Payload);
                        }

                        try
                        {
                            await eventEnricher.EnrichAsync(enrichedEvent, typed);

                            foreach (var (ruleId, rule, actionHandler) in rules)
                            {
                                if (!triggerHandler.Trigger(enrichedEvent, rule.Trigger))
                                {
                                    // For the simulation we want to proceed as much as possible.
                                    if (context.IncludeSkipped)
                                    {
                                        skipReason |= SkipReason.ConditionDoesNotMatch;
                                    }
                                    else
                                    {
                                        await WriteAsync(ruleId, rule, JobResult.ConditionDoesNotMatch);
                                        continue;
                                    }
                                }

                                var result = await CreateJobAsync(actionHandler, enrichedEvent, ruleId, rule, now);

                                // If the conditions matchs, we can skip creating a new object and save a few allocation.s
                                if (skipReason != SkipReason.None)
                                {
                                    result = result with { SkipReason = skipReason };
                                }

                                await WriteAsync(ruleId, rule, result);
                            }
                        }
                        catch (Exception ex)
                        {
                            foreach (var (ruleId, rule, actionHandler) in rules)
                            {
                                if (!hasWrittenForRule.Contains(ruleId))
                                {
                                    await WriteAsync(ruleId, rule, new JobResult
                                    {
                                        EnrichedEvent = enrichedEvent,
                                        EnrichmentError = ex,
                                        SkipReason = SkipReason.Failed
                                    });
                                }
                            }

                            log.LogError(ex, "Failed to create rule jobs from event.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    foreach (var (ruleId, rule, actionHandler) in rules)
                    {
                        if (!hasWrittenForRule.Contains(ruleId))
                        {
                            await WriteAsync(ruleId, rule, JobResult.Failed(ex));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            foreach (var (ruleId, rule) in context.Rules)
            {
                await WriteAsync(ruleId, rule, JobResult.Failed(ex));
            }

            log.LogError(ex, "Failed to create rule job.");
        }
    }

    private async Task<JobResult> CreateJobAsync(IRuleActionHandler actionHandler, EnrichedEvent enrichedEvent, DomainId ruleId, Rule rule, Instant now)
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
            RuleId = ruleId
        };

        try
        {
            var (description, data) = await actionHandler.CreateJobAsync(enrichedEvent, rule.Action);

            var json = serializer.Serialize(data);

            job.ActionData = json;
            job.ActionName = actionName;
            job.Description = description;

            return new JobResult { Job = job, EnrichedEvent = enrichedEvent };
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
