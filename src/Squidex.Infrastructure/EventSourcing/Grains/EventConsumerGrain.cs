// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public class EventConsumerGrain : Grain, IEventConsumerGrain
    {
        private readonly EventConsumerFactory eventConsumerFactory;
        private readonly IStore<string> store;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly IEventStore eventStore;
        private readonly ISemanticLog log;
        private TaskScheduler scheduler;
        private IPersistence<EventConsumerState> persistence;
        private IEventSubscription currentSubscription;
        private IEventConsumer eventConsumer;
        private EventConsumerState state = new EventConsumerState();

        public EventConsumerGrain(
            EventConsumerFactory eventConsumerFactory,
            IStore<string> store,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            ISemanticLog log)
            : this (eventConsumerFactory, store, eventStore, eventDataFormatter, null, null, log)
        {
        }

        protected EventConsumerGrain(
            EventConsumerFactory eventConsumerFactory,
            IStore<string> store,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            IGrainIdentity identity,
            IGrainRuntime runtime,
            ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));
            Guard.NotNull(store, nameof(store));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventDataFormatter, nameof(eventDataFormatter));
            Guard.NotNull(eventConsumerFactory, nameof(eventConsumerFactory));

            this.log = log;

            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.eventConsumerFactory = eventConsumerFactory;

            this.store = store;
        }

        public override Task OnActivateAsync()
        {
            scheduler = TaskScheduler.Current;

            eventConsumer = eventConsumerFactory(this.GetPrimaryKeyString());

            persistence = store.WithSnapshots<EventConsumerState, string>(GetType(), this.GetPrimaryKeyString(), s => state = s);

            return persistence.ReadAsync();
        }

        protected virtual IEventSubscription CreateSubscription(IEventStore eventStore, string streamFilter, string position)
        {
            return new RetrySubscription(eventStore, new WrapperSubscription(this.AsReference<IEventConsumerGrain>(), scheduler), streamFilter, position);
        }

        public Task<Immutable<EventConsumerInfo>> GetStateAsync()
        {
            return Task.FromResult(state.ToInfo(this.eventConsumer.Name).AsImmutable());
        }

        public Task OnEventAsync(Immutable<IEventSubscription> subscription, Immutable<StoredEvent> storedEvent)
        {
            if (subscription.Value != currentSubscription)
            {
                return TaskHelper.Done;
            }

            return DoAndUpdateStateAsync(async () =>
            {
                var @event = ParseKnownEvent(storedEvent.Value);

                if (@event != null)
                {
                    await DispatchConsumerAsync(@event);
                }

                state = state.Handled(storedEvent.Value.EventPosition);
            });
        }

        public Task OnErrorAsync(Immutable<IEventSubscription> subscription, Immutable<Exception> exception)
        {
            if (subscription.Value != currentSubscription)
            {
                return TaskHelper.Done;
            }

            return DoAndUpdateStateAsync(() =>
            {
                Unsubscribe();

                state = state.Failed(exception.Value);
            });
        }

        public Task WakeUpAsync()
        {
            currentSubscription?.WakeUp();

            return TaskHelper.Done;
        }

        public Task ActivateAsync()
        {
            if (!state.IsStopped)
            {
                Subscribe(state.Position);
            }

            return TaskHelper.Done;
        }

        public Task StartAsync()
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

        public Task StopAsync()
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

        public Task ResetAsync()
        {
            return DoAndUpdateStateAsync(async () =>
            {
                Unsubscribe();

                await ClearAsync();

                Subscribe(null);

                state = state.Reset();
            });
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