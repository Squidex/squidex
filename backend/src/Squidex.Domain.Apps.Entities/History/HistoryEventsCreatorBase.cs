// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.History
{
    public abstract class HistoryEventsCreatorBase : IHistoryEventsCreator
    {
        private readonly Dictionary<string, string> texts = new Dictionary<string, string>();
        private readonly TypeNameRegistry typeNameRegistry;

        public IReadOnlyDictionary<string, string> Texts
        {
            get => texts;
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

        protected HistoryEvent ForEvent(IEvent @event, string channel)
        {
            var message = typeNameRegistry.GetName(@event.GetType());

            return new HistoryEvent(channel, message);
        }

        public Task<HistoryEvent?> CreateEventAsync(Envelope<IEvent> @event)
        {
            if (HasEventText(@event.Payload))
            {
                return CreateEventCoreAsync(@event);
            }

            return Task.FromResult<HistoryEvent?>(null);
        }

        protected abstract Task<HistoryEvent?> CreateEventCoreAsync(Envelope<IEvent> @event);
    }
}
