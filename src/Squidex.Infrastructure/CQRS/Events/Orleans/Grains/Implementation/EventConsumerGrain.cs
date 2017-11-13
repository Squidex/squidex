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
using Orleans.Concurrency;
using Orleans.Providers;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Events.Orleans.Grains.Implementation
{
    [StorageProvider(ProviderName = "Default")]
    public class EventConsumerGrain : Grain<EventConsumerGrainState>, IEventSubscriber, IEventConsumerGrain
    {
        private readonly EventDataFormatter eventFormatter;
        private readonly EventConsumerFactory eventConsumerFactory;
        private readonly IEventStore eventStore;
        private readonly ISemanticLog log;
        private IEventSubscription currentSubscription;
        private IEventConsumer eventConsumer;
        private TaskFactory dispatcher;

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

        public override Task OnActivateAsync()
        {
            eventConsumer = eventConsumerFactory(this.GetPrimaryKeyString());

            dispatcher = new TaskFactory(TaskScheduler.Current);

            return TaskHelper.Done;
        }

        public Task ActivateAsync()
        {
            if (!State.IsStopped)
            {
                Subscribe(State.Position);
            }

            return TaskHelper.Done;
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

                State = EventConsumerGrainState.Handled(storedEvent.EventPosition);
            });
        }

        private Task HandleClosedAsync(IEventSubscription subscription)
        {
            if (subscription != currentSubscription)
            {
                return TaskHelper.Done;
            }

            return DoAndUpdateStateAsync(() =>
            {
                Unsubscribe();
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

                State = EventConsumerGrainState.Failed(exception);
            });
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

                State = State.Started();
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

                State = State.Stopped();
            });
        }

        public Task ResetAsync()
        {
            return DoAndUpdateStateAsync(async () =>
            {
                Unsubscribe();

                await ClearAsync();

                Subscribe(null);

                State = EventConsumerGrainState.Initial();
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

        Task IEventSubscriber.OnClosedAsync(IEventSubscription subscription)
        {
            return dispatcher.StartNew(() => this.HandleClosedAsync(subscription)).Unwrap();
        }

        public Task<Immutable<EventConsumerInfo>> GetStateAsync()
        {
            return Task.FromResult(new Immutable<EventConsumerInfo>(State.ToInfo(this.GetPrimaryKeyString())));
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

                State = EventConsumerGrainState.Failed(ex);
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

        protected virtual IEventSubscription CreateSubscription(IEventStore eventStore, string streamFilter, string position)
        {
            return new RetrySubscription(eventStore, this, streamFilter, position);
        }
    }
}