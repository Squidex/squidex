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
using Orleans.Core;
using Orleans.Runtime;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Events.Orleans.Grains.Implementation
{
    public class EventConsumerGrain : GrainV2<EventConsumerGrainState>, IEventConsumerGrain
    {
        private readonly EventDataFormatter eventFormatter;
        private readonly EventConsumerFactory eventConsumerFactory;
        private readonly IEventStore eventStore;
        private readonly ISemanticLog log;
        private TaskScheduler scheduler;
        private IEventConsumer eventConsumer;
        private IEventSubscription eventSubscription;

        protected IEventStore EventStore
        {
            get { return eventStore; }
        }

        public EventConsumerGrain(
            EventDataFormatter eventFormatter,
            EventConsumerFactory eventConsumerFactory,
            IEventStore eventStore,
            ISemanticLog log,
            IGrainRuntime runtime)
            : this(eventFormatter, eventConsumerFactory, eventStore, log, null, runtime, null)
        {
        }

        protected EventConsumerGrain(
            EventDataFormatter eventFormatter,
            EventConsumerFactory eventConsumerFactory,
            IEventStore eventStore,
            ISemanticLog log,
            IGrainIdentity identity,
            IGrainRuntime runtime,
            IStorage<EventConsumerGrainState> storage)
            : base(identity, runtime, storage)
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
            scheduler = TaskScheduler.Current;

            eventConsumer = eventConsumerFactory(this.GetPrimaryKeyString());

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

        public Task OnEventAsync(Immutable<IEventSubscription> subscription, Immutable<StoredEvent> storedEvent)
        {
            if (subscription.Value != eventSubscription)
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

                State = EventConsumerGrainState.Handled(storedEvent.Value.EventPosition);
            });
        }

        public Task OnErrorAsync(Immutable<IEventSubscription> subscription, Immutable<Exception> exception)
        {
            if (subscription.Value != eventSubscription)
            {
                return TaskHelper.Done;
            }

            return DoAndUpdateStateAsync(() =>
            {
                Unsubscribe();

                State = State.Failed(exception.Value);
            });
        }

        public Task OnClosedAsync(Immutable<IEventSubscription> subscription)
        {
            if (subscription.Value != eventSubscription)
            {
                return TaskHelper.Done;
            }

            return DoAndUpdateStateAsync(() =>
            {
                Unsubscribe();
            });
        }

        public Task<Immutable<EventConsumerInfo>> GetStateAsync()
        {
            return Task.FromResult(new Immutable<EventConsumerInfo>(State.ToInfo(this.GetPrimaryKeyString())));
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
            if (eventSubscription != null)
            {
                eventSubscription.StopAsync().Forget();
                eventSubscription = null;
            }
        }

        private void Subscribe(string position)
        {
            if (eventSubscription == null)
            {
                eventSubscription?.StopAsync().Forget();
                eventSubscription = CreateSubscription(eventConsumer.EventsFilter, position);
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

        protected virtual IEventConsumerGrain GetSelf()
        {
            return this.AsReference<IEventConsumerGrain>();
        }

        protected virtual IEventSubscription CreateSubscription(IEventSubscriber subscriber, string streamFilter, string position)
        {
            return new RetrySubscription(EventStore, subscriber, streamFilter, position);
        }

        private IEventSubscription CreateSubscription(string streamFilter, string position)
        {
            return CreateSubscription(new WrapperSubscription(GetSelf(), scheduler), streamFilter, position);
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

                State = State.Failed(ex);
            }

            await WriteStateAsync();
        }
    }
}