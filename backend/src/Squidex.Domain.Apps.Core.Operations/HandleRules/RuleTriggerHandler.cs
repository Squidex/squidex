// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public abstract class RuleTriggerHandler<TTrigger, TEvent, TEnrichedEvent> : IRuleTriggerHandler
        where TTrigger : RuleTrigger
        where TEvent : AppEvent
        where TEnrichedEvent : EnrichedEvent
    {
        private readonly List<EnrichedEvent> emptyEnrichedEvents = new List<EnrichedEvent>();

        public Type TriggerType
        {
            get { return typeof(TTrigger); }
        }

        public virtual bool CanCreateSnapshotEvents
        {
            get { return false; }
        }

        public virtual async IAsyncEnumerable<EnrichedEvent> CreateSnapshotEvents(TTrigger trigger, DomainId appId)
        {
            await Task.Yield();
            yield break;
        }

        public virtual async Task<List<EnrichedEvent>> CreateEnrichedEventsAsync(Envelope<AppEvent> @event)
        {
            var enrichedEvent = await CreateEnrichedEventAsync(@event.To<TEvent>());

            if (enrichedEvent != null)
            {
                return new List<EnrichedEvent>
                {
                    enrichedEvent
                };
            }
            else
            {
                return emptyEnrichedEvents;
            }
        }

        IAsyncEnumerable<EnrichedEvent> IRuleTriggerHandler.CreateSnapshotEvents(RuleTrigger trigger, DomainId appId)
        {
            return CreateSnapshotEvents((TTrigger)trigger, appId);
        }

        bool IRuleTriggerHandler.Trigger(EnrichedEvent @event, RuleTrigger trigger)
        {
            if (@event is TEnrichedEvent typed)
            {
                return Trigger(typed, (TTrigger)trigger);
            }

            return false;
        }

        bool IRuleTriggerHandler.Trigger(AppEvent @event, RuleTrigger trigger, DomainId ruleId)
        {
            if (@event is TEvent typed)
            {
                return Trigger(typed, (TTrigger)trigger, ruleId);
            }

            return false;
        }

        protected virtual Task<TEnrichedEvent?> CreateEnrichedEventAsync(Envelope<TEvent> @event)
        {
            return Task.FromResult<TEnrichedEvent?>(null);
        }

        protected abstract bool Trigger(TEnrichedEvent @event, TTrigger trigger);

        protected virtual bool Trigger(TEvent @event, TTrigger trigger, DomainId ruleId)
        {
            return true;
        }
    }
}
