// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public class RuleService
    {
        private const string ContentPrefix = "Content";
        private readonly Dictionary<Type, IRuleActionHandler> ruleActionHandlers;
        private readonly Dictionary<Type, IRuleTriggerHandler> ruleTriggerHandlers;
        private readonly TypeNameRegistry typeNameRegistry;
        private readonly IClock clock;

        public RuleService(
            IEnumerable<IRuleTriggerHandler> ruleTriggerHandlers,
            IEnumerable<IRuleActionHandler> ruleActionHandlers,
            IClock clock,
            TypeNameRegistry typeNameRegistry)
        {
            Guard.NotNull(ruleTriggerHandlers, nameof(ruleTriggerHandlers));
            Guard.NotNull(ruleActionHandlers, nameof(ruleActionHandlers));
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));
            Guard.NotNull(clock, nameof(clock));

            this.typeNameRegistry = typeNameRegistry;

            this.ruleTriggerHandlers = ruleTriggerHandlers.ToDictionary(x => x.TriggerType);
            this.ruleActionHandlers = ruleActionHandlers.ToDictionary(x => x.ActionType);

            this.clock = clock;
        }

        public virtual RuleJob CreateJob(Rule rule, Envelope<IEvent> @event)
        {
            Guard.NotNull(rule, nameof(rule));
            Guard.NotNull(@event, nameof(@event));

            if (!(@event.Payload is AppEvent appEvent))
            {
                return null;
            }

            var actionType = rule.Action.GetType();

            if (!ruleTriggerHandlers.TryGetValue(rule.Trigger.GetType(), out var triggerHandler))
            {
                return null;
            }

            if (!ruleActionHandlers.TryGetValue(actionType, out var actionHandler))
            {
                return null;
            }

            var appEventEnvelope = @event.To<AppEvent>();

            if (!triggerHandler.Triggers(appEventEnvelope, rule.Trigger))
            {
                return null;
            }

            var eventName = CreateEventName(appEvent);

            var now = clock.GetCurrentInstant();

            var actionName = typeNameRegistry.GetName(actionType);
            var actionData = actionHandler.CreateJob(appEventEnvelope, eventName, rule.Action);

            var eventTime =
                @event.Headers.Contains(CommonHeaders.Timestamp) ?
                @event.Headers.Timestamp() :
                now;

            var aggregateId =
                @event.Headers.Contains(CommonHeaders.AggregateId) ?
                @event.Headers.AggregateId() :
                Guid.NewGuid();

            var job = new RuleJob
            {
                JobId = Guid.NewGuid(),
                ActionName = actionName,
                ActionData = actionData.Data,
                AggregateId = aggregateId,
                AppId = appEvent.AppId.Id,
                Created = now,
                EventName = eventName,
                Expires = eventTime.Plus(Constants.ExpirationTime),
                Description = actionData.Description
            };

            if (job.Expires < now)
            {
                return null;
            }

            return job;
        }

        public virtual async Task<(string Dump, RuleResult Result, TimeSpan Elapsed)> InvokeAsync(string actionName, RuleJobData job)
        {
            try
            {
                var actionType = typeNameRegistry.GetType(actionName);
                var actionWatch = Stopwatch.StartNew();

                var result = await ruleActionHandlers[actionType].ExecuteJobAsync(job);

                actionWatch.Stop();

                var dumpBuilder = new StringBuilder(result.Dump);

                dumpBuilder.AppendLine();
                dumpBuilder.AppendFormat("Elapsed {0}.", actionWatch.Elapsed);
                dumpBuilder.AppendLine();

                if (result.Exception is TimeoutException || result.Exception is OperationCanceledException)
                {
                    dumpBuilder.AppendLine();
                    dumpBuilder.AppendLine("Action timed out.");

                    return (dumpBuilder.ToString(), RuleResult.Timeout, actionWatch.Elapsed);
                }
                else if (result.Exception != null)
                {
                    return (dumpBuilder.ToString(), RuleResult.Failed, actionWatch.Elapsed);
                }
                else
                {
                    return (dumpBuilder.ToString(), RuleResult.Success, actionWatch.Elapsed);
                }
            }
            catch (Exception ex)
            {
                return (ex.ToString(), RuleResult.Failed, TimeSpan.Zero);
            }
        }

        private string CreateEventName(AppEvent appEvent)
        {
            var eventName = typeNameRegistry.GetName(appEvent.GetType());

            if (appEvent is SchemaEvent schemaEvent)
            {
                if (eventName.StartsWith(ContentPrefix, StringComparison.Ordinal))
                {
                    eventName = eventName.Substring(ContentPrefix.Length);
                }

                return $"{schemaEvent.SchemaId.Name.ToPascalCase()}{eventName}";
            }

            return eventName;
        }
    }
}
