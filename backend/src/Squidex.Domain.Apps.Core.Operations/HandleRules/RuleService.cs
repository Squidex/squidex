// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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
            Guard.NotNull(jsonSerializer, nameof(jsonSerializer));
            Guard.NotNull(ruleOptions, nameof(ruleOptions));
            Guard.NotNull(ruleTriggerHandlers, nameof(ruleTriggerHandlers));
            Guard.NotNull(ruleActionHandlers, nameof(ruleActionHandlers));
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));
            Guard.NotNull(eventEnricher, nameof(eventEnricher));
            Guard.NotNull(clock, nameof(clock));
            Guard.NotNull(log, nameof(log));

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

        public async IAsyncEnumerable<CreatedJob> CreateSnapshotJobsAsync(RuleContext context,
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
                CreatedJob? job;
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
                    job = new CreatedJob(null, ex);
                }

                yield return job;
            }
        }

        public async IAsyncEnumerable<CreatedJob> CreateJobsAsync(Envelope<IEvent> @event, RuleContext context,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            Guard.NotNull(@event, nameof(@event));

            var jobs = new List<CreatedJob>();

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

        private async Task AddJobsAsync(List<CreatedJob> jobs, Envelope<IEvent> @event, RuleContext context, CancellationToken ct)
        {
            try
            {
                var rule = context.Rule;

                if (!rule.IsEnabled)
                {
                    jobs.Add(new CreatedJob(null, null, SkipReason.Disabled));
                    return;
                }

                if (@event.Payload is not AppEvent)
                {
                    jobs.Add(new CreatedJob(null, null, SkipReason.EventMismatch));
                    return;
                }

                var typed = @event.To<AppEvent>();

                if (typed.Payload.FromRule)
                {
                    jobs.Add(new CreatedJob(null, null, SkipReason.FromRule));
                    return;
                }

                var actionType = rule.Action.GetType();

                if (!ruleTriggerHandlers.TryGetValue(rule.Trigger.GetType(), out var triggerHandler))
                {
                    jobs.Add(new CreatedJob(null, null, SkipReason.NoTrigger));
                    return;
                }

                if (!ruleActionHandlers.TryGetValue(actionType, out var actionHandler))
                {
                    jobs.Add(new CreatedJob(null, null, SkipReason.NoAction));
                    return;
                }

                var now = clock.GetCurrentInstant();

                var eventTime =
                    @event.Headers.ContainsKey(CommonHeaders.Timestamp) ?
                    @event.Headers.Timestamp() :
                    now;

                if (context.IgnoreStale && eventTime.Plus(Constants.StaleTime) < now)
                {
                    jobs.Add(new CreatedJob(null, null, SkipReason.TooOld));
                    return;
                }

                if (!triggerHandler.Trigger(typed, context))
                {
                    jobs.Add(new CreatedJob(null, null, SkipReason.ConditionDoesNotMatch));
                    return;
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
                            if (jobs.Count == 0)
                            {
                                jobs.Add(new CreatedJob(null, null, SkipReason.ConditionDoesNotMatch));
                            }

                            continue;
                        }

                        var job = await CreateJobAsync(actionHandler, enrichedEvent, context, now);

                        jobs.Add(job);
                    }
                    catch (Exception ex)
                    {
                        if (jobs.Count == 0)
                        {
                            jobs.Add(new CreatedJob(null, null, SkipReason.Failed));
                        }

                        log.LogError(ex, w => w
                            .WriteProperty("action", "createRuleJobFromEvent")
                            .WriteProperty("status", "Failed"));
                    }
                }
            }
            catch (Exception ex)
            {
                jobs.Add(new CreatedJob(null, null, SkipReason.Failed));

                log.LogError(ex, w => w
                    .WriteProperty("action", "createRuleJob")
                    .WriteProperty("status", "Failed"));
            }
        }

        private async Task<CreatedJob> CreateJobAsync(IRuleActionHandler actionHandler, EnrichedEvent enrichedEvent, RuleContext context, Instant now)
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

                return new CreatedJob(job, null);
            }
            catch (Exception ex)
            {
                job.Description = "Failed to create job";

                return new CreatedJob(job, ex);
            }
        }

        public string GetName(AppEvent @event)
        {
            foreach (var handler in ruleTriggerHandlers.Values)
            {
                var name = handler.GetName(@event);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }
            }

            return @event.GetType().Name;
        }

        public async Task<(Result Result, TimeSpan Elapsed)> InvokeAsync(string actionName, string job)
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
                    result = await actionHandler.ExecuteJobAsync(deserialized, cts.Token).WithCancellation(cts.Token);
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
