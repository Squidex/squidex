// ==========================================================================
//  EventConsumerGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Events.Grains
{
    public class EventConsumerGrain : Grain<EventConsumerInfo>, IEventSubscriber, IEventConsumerGrain
    {
        private readonly EventDataFormatter eventFormatter;
        private readonly EventConsumerFactory eventConsumerFactory;
        private readonly IEventStore eventStore;
        private readonly ISemanticLog log;
        private TaskFactory dispatcher;
        private IEventSubscription currentSubscription;
        private IEventConsumer eventConsumer;

        public EventConsumerGrain(
            EventDataFormatter eventFormatter,
            EventConsumerFactory eventConsumerFactory,
            IEventStore eventStore,
            ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventFormatter, nameof(eventFormatter));
            Guard.NotNull(eventConsumerFactory, nameof(eventConsumerFactory));

            this.log = log;

            this.eventStore = eventStore;
            this.eventFormatter = eventFormatter;
            this.eventConsumerFactory = eventConsumerFactory;
        }

        public override async Task OnActivateAsync()
        {
            dispatcher = new TaskFactory(TaskScheduler.Current);

            await GrainFactory.GetGrain<IEventConsumerRegistryGrain>(string.Empty).RegisterAsync(this.IdentityString);

            eventConsumer = eventConsumerFactory(this.IdentityString);

            if (!State.IsStopped)
            {
                Subscribe(State.Position);
            }
        }

        protected virtual IEventSubscription CreateSubscription(IEventStore eventStore, string streamFilter, string position)
        {
            return new RetrySubscription(eventStore, this, streamFilter, position);
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

                State.Error = null;
                State.Position = storedEvent.EventPosition;
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

                State.Error = exception.ToString();
                State.IsStopped = true;
            });
        }

        public Task<EventConsumerInfo> GetStateAsync()
        {
            return Task.FromResult(State);
        }

        public Task StartAsync()
        {
            if (!State.IsStopped)
            {
                return TaskHelper.Done;
            }

            return DoAndUpdateStateAsync(() =>
            {
                Subscribe(State.Position);

                State.Error = null;
                State.IsStopped = false;
            });
        }

        public Task StopAsync()
        {
            if (State.IsStopped)
            {
                return TaskHelper.Done;
            }

            return DoAndUpdateStateAsync(() =>
            {
                Unsubscribe();

                State.Error = null;
                State.IsStopped = true;
            });
        }

        public Task ResetAsync()
        {
            return DoAndUpdateStateAsync(async () =>
            {
                Unsubscribe();

                await ClearAsync();

                Subscribe(null);

                State.Error = null;
                State.Position = null;
                State.IsStopped = false;
            });
        }

        Task IEventSubscriber.OnEventAsync(IEventSubscription subscription, StoredEvent storedEvent)
        {
            return dispatcher.StartNew(() => this.HandleEventAsync(subscription, storedEvent)).Unwrap();
        }

        Task IEventSubscriber.OnErrorAsync(IEventSubscription subscription, Exception exception)
        {
            return dispatcher.StartNew(() => this.HandleErrorAsync(subscription, exception)).Unwrap();
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

                State.Error = ex.ToString();
                State.IsStopped = true;
            }

            await WriteStateAsync();
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
                var @event = eventFormatter.Parse(message.Data);

                @event.SetEventPosition(message.EventPosition);
                @event.SetEventStreamNumber(message.EventStreamNumber);

                return @event;
            }
            catch (TypeNameNotFoundException)
            {
                log.LogDebug(w => w.WriteProperty("oldEventFound", message.Data.Type));

                return null;
            }
        }
    }
}