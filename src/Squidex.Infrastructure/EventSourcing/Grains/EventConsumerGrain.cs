// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public class EventConsumerGrain : DisposableObjectBase, IStatefulObject<string>, IEventSubscriber
    {
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly IStore<string> store;
        private readonly IEventStore eventStore;
        private readonly ISemanticLog log;
        private readonly SingleThreadedDispatcher dispatcher = new SingleThreadedDispatcher(1);
        private IEventSubscription currentSubscription;
        private IEventConsumer eventConsumer;
        private IPersistence<EventConsumerState> persistence;
        private EventConsumerState state = new EventConsumerState();

        public EventConsumerGrain(
            IStore<string> store,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));
            Guard.NotNull(store, nameof(store));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventDataFormatter, nameof(eventDataFormatter));

            this.log = log;
            this.store = store;
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                dispatcher.StopAndWaitAsync().Wait();
            }
        }

        public Task ActivateAsync(string key)
        {
            persistence = store.WithSnapshots<EventConsumerGrain, EventConsumerState, string>(key, s => state = s);

            return persistence.ReadAsync();
        }

        protected virtual IEventSubscription CreateSubscription(IEventStore eventStore, string streamFilter, string position)
        {
            return new RetrySubscription(eventStore, this, streamFilter, position);
        }

        public virtual EventConsumerInfo GetState()
        {
            return state.ToInfo(this.eventConsumer.Name);
        }

        public virtual void Stop()
        {
            dispatcher.DispatchAsync(HandleStopAsync).Forget();
        }

        public virtual void Start()
        {
            dispatcher.DispatchAsync(HandleStartAsync).Forget();
        }

        public virtual void Reset()
        {
            dispatcher.DispatchAsync(HandleResetAsync).Forget();
        }

        public virtual void Activate(IEventConsumer eventConsumer)
        {
            Guard.NotNull(eventConsumer, nameof(eventConsumer));

            dispatcher.DispatchAsync(() => HandleSetupAsync(eventConsumer)).Forget();
        }

        private Task HandleSetupAsync(IEventConsumer consumer)
        {
            eventConsumer = consumer;

            if (!state.IsStopped)
            {
                Subscribe(state.Position);
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

                state = state.Handled(storedEvent.EventPosition);
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

                state = state.Failed(exception);
            });
        }

        private Task HandleStartAsync()
        {
            if (!state.IsStopped)
            {
                return TaskHelper.Done;
            }

            return DoAndUpdateStateAsync(() =>
            {
                Subscribe(state.Position);

                state = state.Started();
            });
        }

        private Task HandleStopAsync()
        {
            if (state.IsStopped)
            {
                return TaskHelper.Done;
            }

            return DoAndUpdateStateAsync(() =>
            {
                Unsubscribe();

                state = state.Stopped();
            });
        }

        private Task HandleResetAsync()
        {
            return DoAndUpdateStateAsync(async () =>
            {
                Unsubscribe();

                await ClearAsync();

                Subscribe(null);

                state = state.Reset();
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

        private Task DoAndUpdateStateAsync(Action action, [CallerMemberName] string caller = null)
        {
            return DoAndUpdateStateAsync(() => { action(); return TaskHelper.Done; }, caller);
        }

        private async Task DoAndUpdateStateAsync(Func<Task> action, [CallerMemberName] string caller = null)
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
                    .WriteProperty("action", caller)
                    .WriteProperty("status", "Failed")
                    .WriteProperty("eventConsumer", eventConsumer.Name));

                state = state.Failed(ex);
            }

            await persistence.WriteSnapshotAsync(state);
        }

        private async Task ClearAsync()
        {
            var actionId = Guid.NewGuid().ToString();

            log.LogInformation(w => w
                .WriteProperty("action", "EventConsumerReset")
                .WriteProperty("actionId", actionId)
                .WriteProperty("status", "Started")
                .WriteProperty("eventConsumer", eventConsumer.Name));

            using (log.MeasureTrace(w => w
                .WriteProperty("action", "EventConsumerReset")
                .WriteProperty("actionId", actionId)
                .WriteProperty("status", "Completed")
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
                .WriteProperty("status", "Started")
                .WriteProperty("eventId", eventId)
                .WriteProperty("eventType", eventType)
                .WriteProperty("eventConsumer", eventConsumer.Name));

            using (log.MeasureTrace(w => w
                .WriteProperty("action", "HandleEvent")
                .WriteProperty("actionId", eventId)
                .WriteProperty("status", "Completed")
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
                var @event = eventDataFormatter.Parse(message.Data);

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