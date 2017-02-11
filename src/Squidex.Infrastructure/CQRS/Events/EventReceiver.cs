// ==========================================================================
//  EventReceiver.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Squidex.Infrastructure.Timers;

// ReSharper disable MethodSupportsCancellation
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class EventReceiver : DisposableObject
    {
        private readonly EventDataFormatter formatter;
        private readonly IEventStore eventStore;
        private readonly IEventNotifier eventNotifier;
        private readonly ILogger<EventReceiver> logger;
        private CompletionTimer timer;

        public EventReceiver(
            EventDataFormatter formatter,
            IEventStore eventStore, 
            IEventNotifier eventNotifier,
            ILogger<EventReceiver> logger)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventNotifier, nameof(eventNotifier));

            this.logger = logger;
            this.formatter = formatter;
            this.eventStore = eventStore;
            this.eventNotifier = eventNotifier;
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    timer?.Dispose();
                }
                catch (Exception ex)
                {
                    logger.LogCritical(InfrastructureErrors.EventHandlingFailed, ex, "Event stream {0} has been aborted");
                }
            }
        }

        public void Subscribe(IEventCatchConsumer eventConsumer, int delay = 5000)
        {
            Guard.NotNull(eventConsumer, nameof(eventConsumer));

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

                var tcs = new TaskCompletionSource<bool>();

                eventStore.GetEventsAsync(lastReceivedPosition).Subscribe(storedEvent =>
                {
                    var @event = ParseEvent(storedEvent.Data);

                    @event.SetEventNumber(storedEvent.EventNumber);

                    DispatchConsumer(@event, eventConsumer, storedEvent.EventNumber).Wait();

                    lastReceivedPosition++;
                }, ex =>
                {
                    tcs.SetException(ex);
                }, () =>
                {
                    tcs.SetResult(true);
                }, ct);

                await tcs.Task;
            });

            eventNotifier.Subscribe(timer.Trigger);
        }

        private async Task DispatchConsumer(Envelope<IEvent> @event, IEventCatchConsumer consumer, long eventNumber)
        {
            try
            {
                await consumer.On(@event, eventNumber);

                logger.LogDebug("[{0}]: Handled event {1} ({2})", consumer, @event.Payload, @event.Headers.EventId());
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