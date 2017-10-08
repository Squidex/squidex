// ==========================================================================
//  EventConsumerActor.cs
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
    public sealed class EventConsumerActor : Actor, IEventSubscriber, IActor
    {
        private readonly EventDataFormatter formatter;
        private readonly IEventStore eventStore;
        private readonly IEventConsumerInfoRepository eventConsumerInfoRepository;
        private readonly ISemanticLog log;
        private IEventSubscription eventSubscription;
        private IEventConsumer eventConsumer;
        private bool isRunning;
        private bool isSetup;

        private sealed class Setup
        {
            public IEventConsumer EventConsumer { get; set; }
        }

        private abstract class SubscriptionMessage
        {
            public IEventSubscription Subscription { get; set; }
        }

        private sealed class SubscriptionEventReceived : SubscriptionMessage
        {
            public StoredEvent Event { get; set; }
        }

        private sealed class SubscriptionFailed : SubscriptionMessage
        {
            public Exception Exception { get; set; }
        }

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

        public Task SubscribeAsync(IEventConsumer eventConsumer)
        {
            Guard.NotNull(eventConsumer, nameof(eventConsumer));

            return DispatchAsync(new Setup { EventConsumer = eventConsumer });
        }

        protected override async Task OnStop()
        {
            if (eventSubscription != null)
            {
                await eventSubscription.StopAsync();
            }
        }

        protected override async Task OnError(Exception exception)
        {
            log.LogError(exception, w => w
                .WriteProperty("action", "HandleEvent")
                .WriteProperty("state", "Failed")
                .WriteProperty("eventConsumer", eventConsumer.Name));

            await StopAsync(exception);

            isRunning = false;
        }

        Task IEventSubscriber.OnEventAsync(IEventSubscription subscription, StoredEvent @event)
        {
            return DispatchAsync(new SubscriptionEventReceived { Subscription = subscription, Event = @event });
        }

        Task IEventSubscriber.OnErrorAsync(IEventSubscription subscription, Exception exception)
        {
            return DispatchAsync(new SubscriptionFailed { Subscription = subscription, Exception = exception });
        }

        void IActor.Tell(object message)
        {
            DispatchAsync(message).Forget();
        }

        protected override async Task OnMessage(object message)
        {
            switch (message)
            {
                case Setup setup when !isSetup:
                    {
                        eventConsumer = setup.EventConsumer;

                        await SetupAsync();

                        isSetup = true;

                        break;
                    }

                case StartConsumerMessage startConsumer when isSetup && !isRunning:
                    {
                        await StartAsync();

                        isRunning = true;

                        break;
                    }

                case StopConsumerMessage stopConsumer when isSetup && isRunning:
                    {
                        await StopAsync();

                        isRunning = false;

                        break;
                    }

                case ResetConsumerMessage resetConsumer when isSetup:
                    {
                        await StopAsync();
                        await ResetAsync();
                        await StartAsync();

                        isRunning = true;

                        break;
                    }

                case SubscriptionFailed subscriptionFailed when isSetup:
                    {
                        if (subscriptionFailed.Subscription == eventSubscription)
                        {
                            await FailAsync(subscriptionFailed.Exception);
                        }

                        break;
                    }

                case SubscriptionEventReceived eventReceived when isSetup:
                    {
                        if (eventReceived.Subscription == eventSubscription)
                        {
                            var @event = ParseEvent(eventReceived.Event);

                            await DispatchConsumerAsync(@event, eventReceived.Event.EventPosition);
                        }

                        break;
                    }
            }
        }

        private async Task SetupAsync()
        {
            await eventConsumerInfoRepository.CreateAsync(eventConsumer.Name);

            var status = await eventConsumerInfoRepository.FindAsync(eventConsumer.Name);

            if (!status.IsStopped)
            {
                DispatchAsync(new StartConsumerMessage()).Forget();
            }
        }

        private async Task StartAsync()
        {
            var status = await eventConsumerInfoRepository.FindAsync(eventConsumer.Name);

            eventSubscription = eventStore.CreateSubscription(this, eventConsumer.EventsFilter, status.Position);

            await eventConsumerInfoRepository.StartAsync(eventConsumer.Name);
        }

        private async Task StopAsync(Exception exception = null)
        {
            eventSubscription?.StopAsync().Forget();
            eventSubscription = null;

            await eventConsumerInfoRepository.StopAsync(eventConsumer.Name, exception?.ToString());
        }

        private async Task ResetAsync()
        {
            var actionId = Guid.NewGuid().ToString();

            log.LogInformation(w => w
                .WriteProperty("action", "EventConsumerReset")
                .WriteProperty("actionId", actionId)
                .WriteProperty("state", "Started")
                .WriteProperty("eventConsumer", eventConsumer.Name));

            using (log.MeasureTrace(w => w
                .WriteProperty("action", "EventConsumerReset")
                .WriteProperty("actionId", actionId)
                .WriteProperty("state", "Completed")
                .WriteProperty("eventConsumer", eventConsumer.Name)))
            {
                await eventConsumerInfoRepository.ResetAsync(eventConsumer.Name);
                await eventConsumer.ClearAsync();
                await eventConsumerInfoRepository.SetPositionAsync(eventConsumer.Name, null, true);
            }
        }

        private async Task DispatchConsumerAsync(Envelope<IEvent> @event, string position)
        {
            var eventId = @event.Headers.EventId().ToString();
            var eventType = @event.Payload.GetType().Name;

            log.LogInformation(w => w
                .WriteProperty("action", "HandleEvent")
                .WriteProperty("actionId", eventId)
                .WriteProperty("state", "Started")
                .WriteProperty("eventId", eventId)
                .WriteProperty("eventType", eventType)
                .WriteProperty("eventConsumer", eventConsumer.Name));

            using (log.MeasureTrace(w => w
                .WriteProperty("action", "HandleEvent")
                .WriteProperty("actionId", eventId)
                .WriteProperty("state", "Completed")
                .WriteProperty("eventId", eventId)
                .WriteProperty("eventType", eventType)
                .WriteProperty("eventConsumer", eventConsumer.Name)))
            {
                await eventConsumer.On(@event);
                await eventConsumerInfoRepository.SetPositionAsync(eventConsumer.Name, position, false);
            }
        }

        private Envelope<IEvent> ParseEvent(StoredEvent message)
        {
            var @event = formatter.Parse(message.Data);

            @event.SetEventPosition(message.EventPosition);
            @event.SetEventStreamNumber(message.EventStreamNumber);

            return @event;
        }
    }
}