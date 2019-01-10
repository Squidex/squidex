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
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

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

        Task<bool> IRuleTriggerHandler.TriggersAsync(EnrichedEvent @event, RuleTrigger trigger)
        {
            return @event is TEnrichedEvent e ? TriggersAsync(e, (TTrigger)trigger) : TaskHelper.False;
        }

        Task<bool> IRuleTriggerHandler.TriggersAsync(IEvent @event, RuleTrigger trigger)
        {
            return @event is TEvent e ? TriggersAsync(e, (TTrigger)trigger) : TaskHelper.False;
        }

        protected abstract Task<bool> TriggersAsync(TEnrichedEvent @event, TTrigger trigger);

        protected virtual Task<bool> TriggersAsync(TEvent @event, TTrigger trigger)
        {
            return TaskHelper.True;
        }
    }
}
