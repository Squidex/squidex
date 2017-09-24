// ==========================================================================
//  EventReceiver.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Actors;
using Squidex.Infrastructure.CQRS.Events.Actors.Messages;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Events.Actors
{
    public sealed class EventConsumerActor : Actor
    {
        private readonly EventDataFormatter formatter;
        private readonly IEventStore eventStore;
        private readonly IEventConsumerInfoRepository eventConsumerInfoRepository;
        private readonly ISemanticLog log;
        private IEventSubscription eventSubscription;
        private IEventConsumer eventConsumer;
        private string position;

        public EventConsumerActor(
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

        public void Subscribe(IEventConsumer eventConsumer)
        {
            Guard.NotNull(eventConsumer, nameof(eventConsumer));

            this.eventConsumer = eventConsumer;
        }

        protected override async Task OnStop()
        {
            if (eventSubscription != null)
            {
                await eventSubscription.StopAsync();
            }
        }

        protected override Task OnError(Exception exception)
        {
            return StopAsync(exception);
        }

        protected override async Task OnMessage(IMessage message)
        {
            switch (message)
            {
                case StopReceiverMessage stopReceiver:
                    {
                        await StopAsync(stopReceiver.Exception);

                        break;
                    }

                case StartReceiverMessage startReceiver:
                    {
                        await StartAsync();

                        break;
                    }

                case ResetReceiverMessage resetReceiver:
                    {
                        await StopAsync();
                        await ResetAsync();
                        await StartAsync();

                        break;
                    }

                case ReceiveEventMessage receiveEvent:
                    {
                        await DispatchConsumerAsync(ParseEvent(receiveEvent.Event));

                        break;
                    }
            }
        }

        private async Task StartAsync()
        {
            await eventConsumerInfoRepository.CreateAsync(eventConsumer.Name);

            position = (await eventConsumerInfoRepository.FindAsync(eventConsumer.Name)).Position;

            eventSubscription = eventStore.CreateSubscription();
            eventSubscription.SendAsync(new SubscribeMessage { Parent = this, StreamFilter = eventConsumer.EventsFilter, Position = position }).Forget();
        }

        private async Task StopAsync(Exception exception = null)
        {
            if (eventSubscription != null)
            {
                await eventSubscription.StopAsync();
            }

            await eventConsumerInfoRepository.StopAsync(eventConsumer.Name, exception?.Message);
        }

        private async Task ResetAsync()
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

        private async Task DispatchConsumerAsync(Envelope<IEvent> @event)
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
                    .WriteProperty("eventConsumer", eventConsumer.Name));

                await eventConsumer.On(@event);

                log.LogInformation(w => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("actionId", eventId)
                    .WriteProperty("state", "Completed")
                    .WriteProperty("eventId", eventId)
                    .WriteProperty("eventType", eventType)
                    .WriteProperty("eventConsumer", eventConsumer.Name));
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("actionId", eventId)
                    .WriteProperty("state", "Started")
                    .WriteProperty("eventId", eventId)
                    .WriteProperty("eventType", eventType)
                    .WriteProperty("eventConsumer", eventConsumer.Name));

                throw;
            }
        }

        private Envelope<IEvent> ParseEvent(StoredEvent message)
        {
            try
            {
                var @event = formatter.Parse(message.Data);

                @event.SetEventPosition(message.EventPosition);
                @event.SetEventStreamNumber(message.EventStreamNumber);

                return @event;
            }
            catch (Exception ex)
            {
                log.LogFatal(ex, w => w
                    .WriteProperty("action", "ParseEvent")
                    .WriteProperty("state", "Failed")
                    .WriteProperty("eventId", message.Data.EventId.ToString())
                    .WriteProperty("eventPosition", message.EventPosition));

                throw;
            }
        }
    }
}