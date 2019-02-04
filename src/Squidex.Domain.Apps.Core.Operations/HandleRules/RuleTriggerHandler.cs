// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;

#pragma warning disable IDE0019 // Use pattern matching

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public abstract class RuleTriggerHandler<TTrigger, TEvent, TEnrichedEvent> : IRuleTriggerHandler
        where TTrigger : RuleTrigger
        where TEvent : AppEvent
        where TEnrichedEvent : EnrichedEvent
    {
        public Type TriggerType
        {
            get { return typeof(TTrigger); }
        }

        async Task<EnrichedEvent> IRuleTriggerHandler.CreateEnrichedEventAsync(Envelope<AppEvent> @event)
        {
            return await CreateEnrichedEventAsync(@event.To<TEvent>());
        }

        bool IRuleTriggerHandler.Trigger(EnrichedEvent @event, RuleTrigger trigger)
        {
            if (@event is TEnrichedEvent typed)
            {
                return Trigger(typed, (TTrigger)trigger);
            }

            return false;
        }

        bool IRuleTriggerHandler.Trigger(AppEvent @event, RuleTrigger trigger, Guid ruleId)
        {
            if (@event is TEvent typed)
            {
                return Trigger(typed, (TTrigger)trigger, ruleId);
            }

            return false;
        }

        protected abstract Task<TEnrichedEvent> CreateEnrichedEventAsync(Envelope<TEvent> @event);

        protected abstract bool Trigger(TEnrichedEvent @event, TTrigger trigger);

        protected virtual bool Trigger(TEvent @event, TTrigger trigger, Guid ruleId)
        {
            return true;
        }
    }
}
