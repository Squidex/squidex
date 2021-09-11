// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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
using Squidex.Log;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public sealed class RuleService : IRuleService
    {
        private readonly Dictionary<Type, IRuleActionHandler> ruleActionHandlers;
        private readonly Dictionary<Type, IRuleTriggerHandler> ruleTriggerHandlers;
        private readonly TypeNameRegistry typeNameRegistry;
        private readonly RuleOptions ruleOptions;
        private readonly IEventEnricher eventEnricher;
        private readonly IJsonSerializer jsonSerializer;
        private readonly IClock clock;
        private readonly ISemanticLog log;

        public RuleService(
            IOptions<RuleOptions> ruleOptions,
            IEnumerable<IRuleTriggerHandler> ruleTriggerHandlers,
            IEnumerable<IRuleActionHandler> ruleActionHandlers,
            IEventEnricher eventEnricher,
            IJsonSerializer jsonSerializer,
            IClock clock,
            ISemanticLog log,
            TypeNameRegistry typeNameRegistry)
        {
            this.typeNameRegistry = typeNameRegistry;

            this.ruleOptions = ruleOptions.Value;
            this.ruleTriggerHandlers = ruleTriggerHandlers.ToDictionary(x => x.TriggerType);
            this.ruleActionHandlers = ruleActionHandlers.ToDictionary(x => x.ActionType);
            this.eventEnricher = eventEnricher;

            this.jsonSerializer = jsonSerializer;

            this.clock = clock;

            this.log = log;
        }

        public bool CanCreateSnapshotEvents(RuleContext context)
        {
            Guard.NotNull(context.Rule, nameof(context.Rule));

            if (!ruleTriggerHandlers.TryGetValue(context.Rule.Trigger.GetType(), out var triggerHandler))
            {
                return false;
            }

            return triggerHandler.CanCreateSnapshotEvents;
        }

        public async IAsyncEnumerable<JobResult> CreateSnapshotJobsAsync(RuleContext context,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            Guard.NotNull(context.Rule, nameof(context.Rule));

            var rule = context.Rule;

            if (!rule.IsEnabled)
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

            var now = clock.GetCurrentInstant();

            await foreach (var enrichedEvent in triggerHandler.CreateSnapshotEventsAsync(context, ct))
            {
                JobResult? job;
                try
                {
                    await eventEnricher.EnrichAsync(enrichedEvent, null);

                    if (!triggerHandler.Trigger(enrichedEvent, context))
                    {
                        continue;
                    }

                    job = await CreateJobAsync(actionHandler, enrichedEvent, context, now);
                }
                catch (Exception ex)
                {
                    job = JobResult.Failed(ex);
                }

                yield return job;
            }
        }

        public async IAsyncEnumerable<JobResult> CreateJobsAsync(Envelope<IEvent> @event, RuleContext context,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            Guard.NotNull(@event, nameof(@event));

            var jobs = new List<JobResult>();

            await AddJobsAsync(jobs, @event, context, ct);

            foreach (var job in jobs)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                yield return job;
            }
        }

        private async Task AddJobsAsync(List<JobResult> jobs, Envelope<IEvent> @event, RuleContext context,
            CancellationToken ct)
        {
            try
            {
                var skipReason = SkipReason.None;

                var rule = context.Rule;

                if (!rule.IsEnabled)
                {
                    // For the simulation we want to proceed as much as possible.
                    if (context.IncludeSkipped)
                    {
                        skipReason |= SkipReason.Disabled;
                    }
                    else
                    {
                        jobs.Add(JobResult.Disabled);
                        return;
                    }
                }

                if (@event.Payload is not AppEvent)
                {
                    jobs.Add(JobResult.WrongEvent);
                    return;
                }

                var typed = @event.To<AppEvent>();

                if (typed.Payload.FromRule)
                {
                    // For the simulation we want to proceed as much as possible.
                    if (context.IncludeSkipped)
                    {
                        skipReason |= SkipReason.FromRule;
                    }
                    else
                    {
                        jobs.Add(JobResult.FromRule);
                        return;
                    }
                }

                var actionType = rule.Action.GetType();

                if (!ruleTriggerHandlers.TryGetValue(rule.Trigger.GetType(), out var triggerHandler))
                {
                    jobs.Add(JobResult.NoTrigger);
                    return;
                }

                if (!triggerHandler.Handles(typed.Payload))
                {
                    jobs.Add(JobResult.WrongEventForTrigger);
                    return;
                }

                if (!ruleActionHandlers.TryGetValue(actionType, out var actionHandler))
                {
                    jobs.Add(JobResult.NoAction);
                    return;
                }

                var now = clock.GetCurrentInstant();

                var eventTime =
                    @event.Headers.ContainsKey(CommonHeaders.Timestamp) ?
                    @event.Headers.Timestamp() :
                    now;

                if (!context.IncludeStale && eventTime.Plus(Constants.StaleTime) < now)
                {
                    // For the simulation we want to proceed as much as possible.
                    if (context.IncludeSkipped)
                    {
                        skipReason |= SkipReason.TooOld;
                    }
                    else
                    {
                        jobs.Add(JobResult.TooOld);
                        return;
                    }
                }

                if (!triggerHandler.Trigger(typed, context))
                {
                    // For the simulation we want to proceed as much as possible.
                    if (context.IncludeSkipped)
                    {
                        skipReason |= SkipReason.ConditionPrecheckDoesNotMatch;
                    }
                    else
                    {
                        jobs.Add(JobResult.ConditionPrecheckDoesNotMatch);
                        return;
                    }
                }

                await foreach (var enrichedEvent in triggerHandler.CreateEnrichedEventsAsync(typed, context, ct))
                {
                    if (string.IsNullOrWhiteSpace(enrichedEvent.Name))
                    {
                        enrichedEvent.Name = GetName(typed.Payload);
                    }

                    try
                    {
                        await eventEnricher.EnrichAsync(enrichedEvent, typed);

                        if (!triggerHandler.Trigger(enrichedEvent, context))
                        {
                            // For the simulation we want to proceed as much as possible.
                            if (context.IncludeSkipped)
                            {
                                skipReason |= SkipReason.ConditionDoesNotMatch;
                            }
                            else
                            {
                                jobs.Add(JobResult.ConditionDoesNotMatch);
                                return;
                            }
                        }

                        var job = await CreateJobAsync(actionHandler, enrichedEvent, context, now);

                        // If the conditions matchs, we can skip creating a new object and save a few allocation.s
                        if (skipReason != SkipReason.None)
                        {
                            job = job with { SkipReason = skipReason };
                        }

                        jobs.Add(job);
                    }
                    catch (Exception ex)
                    {
                        if (jobs.Count == 0)
                        {
                            jobs.Add(new JobResult
                            {
                                EnrichedEvent = enrichedEvent,
                                EnrichmentError = ex,
                                SkipReason = SkipReason.Failed
                            });
                        }

                        log.LogError(ex, w => w
                            .WriteProperty("action", "createRuleJobFromEvent")
                            .WriteProperty("status", "Failed"));
                    }
                }
            }
            catch (Exception ex)
            {
                jobs.Add(JobResult.Failed(ex));

                log.LogError(ex, w => w
                    .WriteProperty("action", "createRuleJob")
                    .WriteProperty("status", "Failed"));
            }
        }

        private async Task<JobResult> CreateJobAsync(IRuleActionHandler actionHandler, EnrichedEvent enrichedEvent, RuleContext context, Instant now)
        {
            var actionName = typeNameRegistry.GetName(context.Rule.Action.GetType());

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
                RuleId = context.RuleId
            };

            try
            {
                var (description, data) = await actionHandler.CreateJobAsync(enrichedEvent, context.Rule.Action);

                var json = jsonSerializer.Serialize(data);

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
            foreach (var handler in ruleTriggerHandlers.Values)
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
                var actionType = typeNameRegistry.GetType(actionName);
                var actionHandler = ruleActionHandlers[actionType];

                var deserialized = jsonSerializer.Deserialize<object>(job, actionHandler.DataType);

                using (var cts = new CancellationTokenSource(GetTimeoutInMs()))
                {
                    using (var combined = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ct))
                    {
                        result = await actionHandler.ExecuteJobAsync(deserialized, combined.Token).WithCancellation(combined.Token);
                    }
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
}
