// ==========================================================================
//  EventReceiver.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Timers;

// ReSharper disable InvertIf

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class EventReceiver : DisposableObjectBase
    {
        private readonly EventDataFormatter formatter;
        private readonly IEventStore eventStore;
        private readonly IEventConsumerInfoRepository eventConsumerInfoRepository;
        private readonly ISemanticLog log;
        private IEventSubscription currentSubscription;
        private CompletionTimer timer;

        public EventReceiver(
            EventDataFormatter formatter,
            IEventStore eventStore,
            IEventConsumerInfoRepository eventConsumerInfoRepository,
            ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventConsumerInfoRepository, nameof(eventConsumerInfoRepository));

            this.log = log;
            this.formatter = formatter;
            this.eventStore = eventStore;
            this.eventConsumerInfoRepository = eventConsumerInfoRepository;
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    currentSubscription?.Dispose();
                }
                catch (Exception ex)
                {
                    log.LogWarning(ex, w => w
                        .WriteProperty("action", "DisposeEventReceiver")
                        .WriteProperty("state", "Failed"));
                }

                try
                {
                    timer?.Dispose();
                }
                catch (Exception ex)
                {
                    log.LogWarning(ex, w => w
                        .WriteProperty("action", "DisposeEventReceiver")
                        .WriteProperty("state", "Failed"));
                }
            }
        }

        public void Refresh()
        {
            ThrowIfDisposed();

            timer?.Wakeup();
        }

        public void Subscribe(IEventConsumer eventConsumer)
        {
            Guard.NotNull(eventConsumer, nameof(eventConsumer));

            ThrowIfDisposed();

            if (timer != null)
            {
                return;
            }

            var consumerName = eventConsumer.Name;
            var consumerStarted = false;

            timer = new CompletionTimer(5000, async ct =>
            {
                if (!consumerStarted)
                {
                    await eventConsumerInfoRepository.CreateAsync(consumerName);

                    consumerStarted = true;
                }

                try
                {
                    var status = await eventConsumerInfoRepository.FindAsync(consumerName);

                    var position = status.Position;

                    if (status.IsResetting)
                    {
                        currentSubscription?.Dispose();
                        currentSubscription = null;

                        position = null;

                        await ResetAsync(eventConsumer);
                    }
                    else if (status.IsStopped)
                    {
                        currentSubscription?.Dispose();
                        currentSubscription = null;

                        return;
                    }
                    
                    if (currentSubscription == null)
                    {
                        await SubscribeAsync(eventConsumer, position);
                    }
                }
                catch (Exception ex)
                {
                    log.LogFatal(ex, w => w.WriteProperty("action", "EventHandlingFailed"));
                }
            });
        }

        private async Task SubscribeAsync(IEventConsumer eventConsumer, string position)
        {
            var consumerName = eventConsumer.Name;

            var subscription = eventStore.CreateSubscription(eventConsumer.EventsFilter, position);

            await subscription.SubscribeAsync(async storedEvent =>
            {
                await DispatchConsumer(ParseEvent(storedEvent), eventConsumer, eventConsumer.Name);

                await eventConsumerInfoRepository.SetPositionAsync(eventConsumer.Name, storedEvent.EventPosition, false);
            }, async exception =>
            {
                await eventConsumerInfoRepository.StopAsync(consumerName, exception.ToString());

                subscription.Dispose();
            });

            currentSubscription = subscription;
        }

        private async Task ResetAsync(IEventConsumer eventConsumer)
        {
            var actionId = Guid.NewGuid().ToString();
            try
            {
                log.LogInformation(w => w
                    .WriteProperty("action", "EventConsumerReset")
                    .WriteProperty("actionId", actionId)
                    .WriteProperty("state", "Started")
                    .WriteProperty("eventConsumer", eventConsumer.Name));

                await eventConsumer.ClearAsync();
                await eventConsumerInfoRepository.SetPositionAsync(eventConsumer.Name, null, true);

                log.LogInformation(w => w
                    .WriteProperty("action", "EventConsumerReset")
                    .WriteProperty("actionId", actionId)
                    .WriteProperty("state", "Completed")
                    .WriteProperty("eventConsumer", eventConsumer.Name));
            }
            catch (Exception ex)
            {
                log.LogFatal(ex, w => w
                    .WriteProperty("action", "EventConsumerReset")
                    .WriteProperty("actionId", actionId)
                    .WriteProperty("state", "Completed")
                    .WriteProperty("eventConsumer", eventConsumer.GetType().Name));

                throw;
            }
        }

        private async Task DispatchConsumer(Envelope<IEvent> @event, IEventConsumer eventConsumer, string consumerName)
        {
            var eventId = @event.Headers.EventId().ToString();
            var eventType = @event.Payload.GetType().Name;
            try
            {
                log.LogInformation(w => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("actionId", eventId)
                    .WriteProperty("state", "Started")
                    .WriteProperty("eventId", eventId)
                    .WriteProperty("eventType", eventType)
                    .WriteProperty("eventConsumer", consumerName));

                await eventConsumer.On(@event);

                log.LogInformation(w => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("actionId", eventId)
                    .WriteProperty("state", "Completed")
                    .WriteProperty("eventId", eventId)
                    .WriteProperty("eventType", eventType)
                    .WriteProperty("eventConsumer", consumerName));
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("actionId", eventId)
                    .WriteProperty("state", "Started")
                    .WriteProperty("eventId", eventId)
                    .WriteProperty("eventType", eventType)
                    .WriteProperty("eventConsumer", consumerName));

                throw;
            }
        }

        private Envelope<IEvent> ParseEvent(StoredEvent storedEvent)
        {
            try
            {
                var @event = formatter.Parse(storedEvent.Data);

                @event.SetEventPosition(storedEvent.EventPosition);
                @event.SetEventStreamNumber(storedEvent.EventStreamNumber);

                return @event;
            }
            catch (Exception ex)
            {
                log.LogFatal(ex, w => w
                    .WriteProperty("action", "ParseEvent")
                    .WriteProperty("state", "Failed")
                    .WriteProperty("eventId", storedEvent.Data.EventId.ToString())
                    .WriteProperty("eventPosition", storedEvent.EventPosition));

                throw;
            }
        }
    }
}