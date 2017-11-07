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
    public class EventConsumerActor : DisposableObjectBase, IEventSubscriber, IActor
    {
        private readonly EventDataFormatter formatter;
        private readonly IEventStore eventStore;
        private readonly IEventConsumerInfoRepository eventConsumerInfoRepository;
        private readonly ISemanticLog log;
        private readonly SingleThreadedDispatcher dispatcher = new SingleThreadedDispatcher(1);
        private IEventSubscription currentSubscription;
        private IEventConsumer eventConsumer;
        private bool statusIsRunning = true;
        private string statusPosition;
        private string statusError;

        private static Func<IEventStore, IEventSubscriber, string, string, IEventSubscription> DefaultFactory
        {
            get { return (e, s, t, p) => new RetrySubscription(e, s, t, p); }
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

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                dispatcher.StopAndWaitAsync().Wait();
            }
        }

        protected virtual IEventSubscription CreateSubscription(IEventStore eventStore, string streamFilter, string position)
        {
            return new RetrySubscription(eventStore, this, streamFilter, position);
        }

        public Task SubscribeAsync(IEventConsumer eventConsumer)
        {
            Guard.NotNull(eventConsumer, nameof(eventConsumer));

            return dispatcher.DispatchAsync(() => HandleSetupAsync(eventConsumer));
        }

        private async Task HandleSetupAsync(IEventConsumer consumer)
        {
            eventConsumer = consumer;

            var status = await eventConsumerInfoRepository.FindAsync(eventConsumer.Name);

            if (status != null)
            {
                statusError = status.Error;
                statusPosition = status.Position;
                statusIsRunning = !status.IsStopped;
            }

            if (statusIsRunning)
            {
                Subscribe(statusPosition);
            }
        }

        private Task HandleEventAsync(IEventSubscription subscription, StoredEvent storedEvent)
        {
            if (subscription != currentSubscription)
            {
                return TaskHelper.Done;
            }

            return DoAndUpdateStateAsync(async () =>
            {
                var @event = ParseKnownEvent(storedEvent);

                if (@event != null)
                {
                    await DispatchConsumerAsync(@event);
                }

                statusError = null;
                statusPosition = storedEvent.EventPosition;
            });
        }

        private Task HandleErrorAsync(IEventSubscription subscription, Exception exception)
        {
            if (subscription != currentSubscription)
            {
                return TaskHelper.Done;
            }

            return DoAndUpdateStateAsync(() =>
            {
                Unsubscribe();

                statusError = exception.ToString();
                statusIsRunning = false;
            });
        }

        private Task HandleStartAsync()
        {
            if (statusIsRunning)
            {
                return TaskHelper.Done;
            }

            return DoAndUpdateStateAsync(() =>
            {
                Subscribe(statusPosition);

                statusError = null;
                statusIsRunning = true;
            });
        }

        private Task HandleStopAsync()
        {
            if (!statusIsRunning)
            {
                return TaskHelper.Done;
            }

            return DoAndUpdateStateAsync(() =>
            {
                Unsubscribe();

                statusError = null;
                statusIsRunning = false;
            });
        }

        private Task HandleResetInternalAsync()
        {
            return DoAndUpdateStateAsync(async () =>
            {
                Unsubscribe();

                await ClearAsync();

                Subscribe(null);

                statusError = null;
                statusPosition = null;
                statusIsRunning = true;
            });
        }

        Task IEventSubscriber.OnEventAsync(IEventSubscription subscription, StoredEvent storedEvent)
        {
            return dispatcher.DispatchAsync(() => HandleEventAsync(subscription, storedEvent));
        }

        Task IEventSubscriber.OnErrorAsync(IEventSubscription subscription, Exception exception)
        {
            return dispatcher.DispatchAsync(() => HandleErrorAsync(subscription, exception));
        }

        void IActor.Tell(object message)
        {
            switch (message)
            {
                case StopConsumerMessage stop:
                    dispatcher.DispatchAsync(() => HandleStopAsync()).Forget();
                    break;

                case StartConsumerMessage stop:
                    dispatcher.DispatchAsync(() => HandleStartAsync()).Forget();
                    break;

                case ResetConsumerMessage stop:
                    dispatcher.DispatchAsync(() => HandleResetInternalAsync()).Forget();
                    break;
            }
        }

        private Task DoAndUpdateStateAsync(Action action)
        {
            return DoAndUpdateStateAsync(() => { action(); return TaskHelper.Done; });
        }

        private async Task DoAndUpdateStateAsync(Func<Task> action)
        {
            try
            {
                await action();
                await eventConsumerInfoRepository.SetAsync(eventConsumer.Name, statusPosition, !statusIsRunning, statusError);
            }
            catch (Exception ex)
            {
                try
                {
                    Unsubscribe();
                }
                catch (Exception unsubscribeException)
                {
                    ex = new AggregateException(ex, unsubscribeException);
                }

                log.LogFatal(ex, w => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("state", "Failed")
                    .WriteProperty("eventConsumer", eventConsumer.Name));

                statusError = ex.ToString();
                statusIsRunning = false;

                await eventConsumerInfoRepository.SetAsync(eventConsumer.Name, statusPosition, !statusIsRunning, statusError);
            }
        }

        private async Task ClearAsync()
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
                await eventConsumer.ClearAsync();
            }
        }

        private async Task DispatchConsumerAsync(Envelope<IEvent> @event)
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
            }
        }

        private void Unsubscribe()
        {
            if (currentSubscription != null)
            {
                currentSubscription.StopAsync().Forget();
                currentSubscription = null;
            }
        }

        private void Subscribe(string position)
        {
            if (currentSubscription == null)
            {
                currentSubscription?.StopAsync().Forget();
                currentSubscription = CreateSubscription(eventStore, eventConsumer.EventsFilter, position);
            }
        }

        private Envelope<IEvent> ParseKnownEvent(StoredEvent message)
        {
            try
            {
                var @event = formatter.Parse(message.Data);

                @event.SetEventPosition(message.EventPosition);
                @event.SetEventStreamNumber(message.EventStreamNumber);

                return @event;
            }
            catch (TypeNameNotFoundException)
            {
                return null;
            }
        }
    }
}