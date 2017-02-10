// ==========================================================================
//  EventReceiver.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Squidex.Infrastructure.Timers;

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class EventReceiver : DisposableObject
    {
        private readonly EventDataFormatter formatter;
        private readonly IEventStore eventStore;
        private readonly IEventNotifier eventNotifier;
        private readonly IEventCatchConsumer eventConsumer;
        private readonly ILogger<EventReceiver> logger;
        private CompletionTimer timer;

        public EventReceiver(
            EventDataFormatter formatter,
            IEventStore eventStore, 
            IEventNotifier eventNotifier,
            IEventCatchConsumer eventConsumer, 
            ILogger<EventReceiver> logger)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventNotifier, nameof(eventNotifier));
            Guard.NotNull(eventConsumer, nameof(eventConsumer));

            this.logger = logger;
            this.formatter = formatter;
            this.eventStore = eventStore;
            this.eventNotifier = eventNotifier;
            this.eventConsumer = eventConsumer;
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                timer?.Dispose();
            }
        }

        public void Subscribe(int delay = 5000)
        {
            if (timer != null)
            {
                return;
            }

            var lastReceivedPosition = long.MinValue;
            
            timer = new CompletionTimer(delay, async ct =>
            {
                if (lastReceivedPosition == long.MinValue)
                {
                    lastReceivedPosition = await eventConsumer.GetLastHandledEventNumber();
                }

                await eventStore.GetEventsAsync(lastReceivedPosition).ForEachAsync(async storedEvent =>
                {
                    var @event = ParseEvent(storedEvent.Data);

                    @event.SetEventNumber(storedEvent.EventNumber);

                    await DispatchConsumer(@event, eventConsumer, storedEvent.EventNumber);
                }, ct);
            });

            eventNotifier.Subscribe(timer.Trigger);
        }

        private async Task DispatchConsumer(Envelope<IEvent> @event, IEventCatchConsumer consumer, long eventNumber)
        {
            try
            {
                await consumer.On(@event, eventNumber);
            }
            catch (Exception ex)
            {
                logger.LogError(InfrastructureErrors.EventHandlingFailed, ex, "[{0}]: Failed to handle event {1} ({2})", consumer, @event.Payload, @event.Headers.EventId());

                throw;
            }
        }

        private Envelope<IEvent> ParseEvent(EventData eventData)
        {
            try
            {
                var @event = formatter.Parse(eventData);

                return @event;
            }
            catch (Exception ex)
            {
                logger.LogError(InfrastructureErrors.EventDeserializationFailed, ex, "Failed to parse event {0}", eventData.EventId);

                throw;
            }
        }
    }
}