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

// ReSharper disable MethodSupportsCancellation
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class EventReceiver : DisposableObjectBase
    {
        private readonly EventDataFormatter formatter;
        private readonly IEventStore eventStore;
        private readonly IEventNotifier eventNotifier;
        private readonly IEventConsumerInfoRepository eventConsumerInfoRepository;
        private readonly ILogger<EventReceiver> logger;
        private CompletionTimer timer;

        public EventReceiver(
            EventDataFormatter formatter,
            IEventStore eventStore, 
            IEventNotifier eventNotifier,
            IEventConsumerInfoRepository eventConsumerInfoRepository,
            ILogger<EventReceiver> logger)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventNotifier, nameof(eventNotifier));
            Guard.NotNull(eventConsumerInfoRepository, nameof(eventConsumerInfoRepository));

            this.logger = logger;
            this.formatter = formatter;
            this.eventStore = eventStore;
            this.eventNotifier = eventNotifier;
            this.eventConsumerInfoRepository = eventConsumerInfoRepository;
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

        public void Trigger()
        {
            timer?.Trigger();
        }

        public void Subscribe(IEventConsumer eventConsumer, int delay = 5000)
        {
            Guard.NotNull(eventConsumer, nameof(eventConsumer));

            if (timer != null)
            {
                return;
            }

            var consumerName = eventConsumer.GetType().Name;
            var consumerStarted = false;
            
            timer = new CompletionTimer(delay, async ct =>
            {
                if (!consumerStarted)
                {
                    await eventConsumerInfoRepository.CreateAsync(consumerName);

                    consumerStarted = true;
                }

                try
                {
                    var status = await eventConsumerInfoRepository.FindAsync(consumerName);

                    var lastHandledEventNumber = status.LastHandledEventNumber;

                    if (status.IsResetting)
                    {
                        await ResetAsync(eventConsumer, consumerName);

                        lastHandledEventNumber = -1;
                    }
                    else if (status.IsStopped)
                    {
                        return;
                    }
                    
                    await eventStore.GetEventsAsync(lastHandledEventNumber)
                        .Select(storedEvent =>
                            {
                                HandleEventAsync(eventConsumer, storedEvent, consumerName).Wait();

                                return storedEvent;
                            }).DefaultIfEmpty();
                }
                catch (Exception ex)
                {
                    logger.LogError(InfrastructureErrors.EventHandlingFailed, ex, "Failed to handle events");

                    await eventConsumerInfoRepository.StopAsync(consumerName, ex.ToString());
                }
            });

            eventNotifier.Subscribe(timer.Trigger);
        }

        private async Task HandleEventAsync(IEventConsumer eventConsumer, StoredEvent storedEvent, string consumerName)
        {
            var @event = ParseEvent(storedEvent);

            await DispatchConsumer(@event, eventConsumer);
            await eventConsumerInfoRepository.SetLastHandledEventNumberAsync(consumerName, storedEvent.EventNumber);
        }

        private async Task ResetAsync(IEventConsumer eventConsumer, string consumerName)
        {
            try
            {
                logger.LogDebug("[{0}]: Reset started", eventConsumer);

                await eventConsumer.ClearAsync();
                await eventConsumerInfoRepository.SetLastHandledEventNumberAsync(consumerName, -1);

                logger.LogDebug("[{0}]: Reset completed", eventConsumer);
            }
            catch (Exception ex)
            {
                logger.LogError(InfrastructureErrors.EventResetFailed, ex, "[{0}]: Reset failed", eventConsumer);

                throw;
            }
        }

        private async Task DispatchConsumer(Envelope<IEvent> @event, IEventConsumer eventConsumer)
        {
            try
            {
                logger.LogDebug("[{0}]: Handling event {1} ({2})", eventConsumer, @event.Payload, @event.Headers.EventId());

                await eventConsumer.On(@event);

                logger.LogDebug("[{0}]: Handled event {1} ({2})", eventConsumer, @event.Payload, @event.Headers.EventId());
            }
            catch (Exception ex)
            {
                logger.LogError(InfrastructureErrors.EventHandlingFailed, ex, "[{0}]: Failed to handle event {1} ({2})", eventConsumer, @event.Payload, @event.Headers.EventId());

                throw;
            }
        }

        private Envelope<IEvent> ParseEvent(StoredEvent storedEvent)
        {
            try
            {
                var @event = formatter.Parse(storedEvent.Data);

                @event.SetEventNumber(storedEvent.EventNumber);
                @event.SetEventStreamNumber(storedEvent.EventStreamNumber);

                return @event;
            }
            catch (Exception ex)
            {
                logger.LogError(InfrastructureErrors.EventDeserializationFailed, ex, "Failed to parse event {0}", storedEvent.Data.EventId);

                throw;
            }
        }
    }
}