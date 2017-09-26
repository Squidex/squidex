// ==========================================================================
//  HistoryEventsCreatorBase.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Read.History
{
    public abstract class HistoryEventsCreatorBase : IHistoryEventsCreator
    {
        private readonly Dictionary<string, string> texts = new Dictionary<string, string>();
        private readonly TypeNameRegistry typeNameRegistry;

        public IReadOnlyDictionary<string, string> Texts
        {
            get { return texts; }
        }

        protected HistoryEventsCreatorBase(TypeNameRegistry typeNameRegistry)
        {
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));

            this.typeNameRegistry = typeNameRegistry;
        }

        protected void AddEventMessage<TEvent>(string message) where TEvent : IEvent
        {
            Guard.NotNullOrEmpty(message, nameof(message));

            texts[typeNameRegistry.GetName<TEvent>()] = message;
        }

        protected bool HasEventText(IEvent @event)
        {
            var message = typeNameRegistry.GetName(@event.GetType());

            return texts.ContainsKey(message);
        }

        protected HistoryEventToStore ForEvent(IEvent @event, string channel)
        {
            var message = typeNameRegistry.GetName(@event.GetType());

            return new HistoryEventToStore(channel, message);
        }

        public Task<HistoryEventToStore> CreateEventAsync(Envelope<IEvent> @event)
        {
            if (HasEventText(@event.Payload))
            {
                return CreateEventCoreAsync(@event);
            }

            return Task.FromResult<HistoryEventToStore>(null);
        }

        protected abstract Task<HistoryEventToStore> CreateEventCoreAsync(Envelope<IEvent> @event);
    }
}
