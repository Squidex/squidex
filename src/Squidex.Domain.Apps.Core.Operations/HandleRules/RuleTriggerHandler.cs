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

        bool IRuleTriggerHandler.Triggers(EnrichedEvent @event, RuleTrigger trigger)
        {
            return @event is TEnrichedEvent e && Triggers(e, (TTrigger)trigger);
        }

        bool IRuleTriggerHandler.Triggers(IEvent @event, RuleTrigger trigger)
        {
            return @event is TEvent e && Triggers(e, (TTrigger)trigger);
        }

        protected abstract bool Triggers(TEnrichedEvent @event, TTrigger trigger);

        protected virtual bool Triggers(TEvent @event, TTrigger trigger)
        {
            return true;
        }
    }
}
