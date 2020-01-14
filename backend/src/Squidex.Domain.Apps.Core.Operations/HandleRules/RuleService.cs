﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NodaTime;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public class RuleService
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
            Guard.NotNull(jsonSerializer);
            Guard.NotNull(ruleOptions);
            Guard.NotNull(ruleTriggerHandlers);
            Guard.NotNull(ruleActionHandlers);
            Guard.NotNull(typeNameRegistry);
            Guard.NotNull(eventEnricher);
            Guard.NotNull(clock);
            Guard.NotNull(log);

            this.typeNameRegistry = typeNameRegistry;

            this.ruleOptions = ruleOptions.Value;
            this.ruleTriggerHandlers = ruleTriggerHandlers.ToDictionary(x => x.TriggerType);
            this.ruleActionHandlers = ruleActionHandlers.ToDictionary(x => x.ActionType);
            this.eventEnricher = eventEnricher;

            this.jsonSerializer = jsonSerializer;

            this.clock = clock;

            this.log = log;
        }

        public virtual async Task<List<RuleJob>> CreateJobsAsync(Rule rule, Guid ruleId, Envelope<IEvent> @event)
        {
            Guard.NotNull(rule);
            Guard.NotNull(@event);

            var result = new List<RuleJob>();

            try
            {
                if (!rule.IsEnabled)
                {
                    return result;
                }

                if (!(@event.Payload is AppEvent))
                {
                    return result;
                }

                var typed = @event.To<AppEvent>();

                var actionType = rule.Action.GetType();

                if (!ruleTriggerHandlers.TryGetValue(rule.Trigger.GetType(), out var triggerHandler))
                {
                    return result;
                }

                if (!ruleActionHandlers.TryGetValue(actionType, out var actionHandler))
                {
                    return result;
                }

                var now = clock.GetCurrentInstant();

                var eventTime =
                    @event.Headers.ContainsKey(CommonHeaders.Timestamp) ?
                    @event.Headers.Timestamp() :
                    now;

                var expires = eventTime.Plus(Constants.ExpirationTime);

                if (eventTime.Plus(Constants.StaleTime) < now)
                {
                    return result;
                }

                if (!triggerHandler.Trigger(typed.Payload, rule.Trigger, ruleId))
                {
                    return result;
                }

                var appEventEnvelope = @event.To<AppEvent>();

                var enrichedEvents = await triggerHandler.CreateEnrichedEventsAsync(appEventEnvelope);

                foreach (var enrichedEvent in enrichedEvents)
                {
                    try
                    {
                        await eventEnricher.EnrichAsync(enrichedEvent, typed);

                        if (!triggerHandler.Trigger(enrichedEvent, rule.Trigger))
                        {
                            continue;
                        }

                        var actionName = typeNameRegistry.GetName(actionType);
                        var actionData = await actionHandler.CreateJobAsync(enrichedEvent, rule.Action);

                        var json = jsonSerializer.Serialize(actionData.Data);

                        var job = new RuleJob
                        {
                            Id = Guid.NewGuid(),
                            ActionData = json,
                            ActionName = actionName,
                            AppId = enrichedEvent.AppId.Id,
                            Created = now,
                            Description = actionData.Description,
                            EventName = enrichedEvent.Name,
                            ExecutionPartition = enrichedEvent.Partition,
                            Expires = expires,
                            RuleId = ruleId
                        };

                        result.Add(job);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, w => w
                            .WriteProperty("action", "createRuleJobFromEvent")
                            .WriteProperty("status", "Failed"));
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "createRuleJob")
                    .WriteProperty("status", "Failed"));
            }

            return result;
        }

        public virtual async Task<(Result Result, TimeSpan Elapsed)> InvokeAsync(string actionName, string job)
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
