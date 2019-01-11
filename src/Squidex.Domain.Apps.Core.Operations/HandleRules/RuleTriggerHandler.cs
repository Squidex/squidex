// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public abstract class RuleTriggerHandler<TTrigger, TEvent, TEnrichedEvent> : IRuleTriggerHandler
        where TTrigger : RuleTrigger
        where TEvent : IEvent
        where TEnrichedEvent : EnrichedEvent
    {
        public Type TriggerType
        {
            get { return typeof(TTrigger); }
        }

        bool IRuleTriggerHandler.Trigger(EnrichedEvent @event, RuleTrigger trigger)
        {
            return @event is TEnrichedEvent e && Trigger(e, (TTrigger)trigger);
        }

        bool IRuleTriggerHandler.Trigger(IEvent @event, RuleTrigger trigger, Guid ruleId)
        {
            return @event is TEvent e && Trigger(e, (TTrigger)trigger, ruleId);
        }

        protected abstract bool Trigger(TEnrichedEvent @event, TTrigger trigger);

        protected virtual bool Trigger(TEvent @event, TTrigger trigger, Guid ruleId)
        {
            return true;
        }
    }
}
