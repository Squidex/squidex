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
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public class EventConsumerGrain : GrainOfString<EventConsumerState>, IEventConsumerGrain
    {
        private readonly EventConsumerFactory eventConsumerFactory;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly IEventStore eventStore;
        private readonly ISemanticLog log;
        private TaskScheduler scheduler;
        private IEventSubscription currentSubscription;
        private IEventConsumer eventConsumer;

        public EventConsumerGrain(
            EventConsumerFactory eventConsumerFactory,
            IStore<string> store,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            ISemanticLog log)
            : base(store)
        {
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventDataFormatter, nameof(eventDataFormatter));
            Guard.NotNull(eventConsumerFactory, nameof(eventConsumerFactory));
            Guard.NotNull(log, nameof(log));

            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.eventConsumerFactory = eventConsumerFactory;

            this.log = log;
        }

        protected override Task OnActivateAsync(string key)
        {
            scheduler = TaskScheduler.Current;

            eventConsumer = eventConsumerFactory(key);

            return TaskHelper.Done;
        }

        public Task<Immutable<EventConsumerInfo>> GetStateAsync()
        {
            return Task.FromResult(CreateInfo());
        }

        private Immutable<EventConsumerInfo> CreateInfo()
        {
            return State.ToInfo(eventConsumer.Name).AsImmutable();
        }

        public Task OnEventAsync(Immutable<IEventSubscription> subscription, Immutable<StoredEvent> storedEvent)
        {
            if (subscription.Value != currentSubscription)
            {
                return TaskHelper.Done;
            }

            return DoAndUpdateStateAsync(async () =>
            {
                if (eventConsumer.Handles(storedEvent.Value))
                {
                    var @event = ParseKnownEvent(storedEvent.Value);

                    if (@event != null)
                    {
                        await DispatchConsumerAsync(@event);
                    }
                }

                State = State.Handled(storedEvent.Value.EventPosition);
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

                State = State.Failed(exception.Value);
            });
        }

        public Task ActivateAsync()
        {
            if (!State.IsStopped)
            {
                Subscribe(State.Position);
            }

            return TaskHelper.Done;
        }

        public async Task<Immutable<EventConsumerInfo>> StartAsync()
        {
            if (!State.IsStopped)
            {
                return CreateInfo();
            }

            await DoAndUpdateStateAsync(() =>
            {
                Subscribe(State.Position);

                State = State.Started();
            });

            return CreateInfo();
        }

        public async Task<Immutable<EventConsumerInfo>> StopAsync()
        {
            if (State.IsStopped)
            {
                return CreateInfo();
            }

            await DoAndUpdateStateAsync(() =>
            {
                Unsubscribe();

                State = State.Stopped();
            });

            return CreateInfo();
        }

        public async Task<Immutable<EventConsumerInfo>> ResetAsync()
        {
            await DoAndUpdateStateAsync(async () =>
            {
                Unsubscribe();

                await ClearAsync();

                Subscribe(null);

                State = State.Reset();
            });

            return CreateInfo();
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

                State = State.Failed(ex);
            }

            await WriteStateAsync();
        }

        private async Task ClearAsync()
        {
            var logContext = (actionId: Guid.NewGuid().ToString(), consumer: eventConsumer.Name);

            log.LogInformation(logContext, (ctx, w) => w
                .WriteProperty("action", "EventConsumerReset")
                .WriteProperty("actionId", ctx.actionId)
                .WriteProperty("status", "Started")
                .WriteProperty("eventConsumer", ctx.consumer));

            using (log.MeasureTrace(logContext, (ctx, w) => w
                .WriteProperty("action", "EventConsumerReset")
                .WriteProperty("actionId", ctx.actionId)
                .WriteProperty("status", "Completed")
                .WriteProperty("eventConsumer", ctx.consumer)))
            {
                await eventConsumer.ClearAsync();
            }
        }

        private async Task DispatchConsumerAsync(Envelope<IEvent> @event)
        {
            var eventId = @event.Headers.EventId().ToString();
            var eventType = @event.Payload.GetType().Name;

            var logContext = (eventId, eventType, consumer: eventConsumer.Name);

            log.LogInformation(logContext, (ctx, w) => w
                .WriteProperty("action", "HandleEvent")
                .WriteProperty("actionId", ctx.eventId)
                .WriteProperty("status", "Started")
                .WriteProperty("eventId", ctx.eventId)
                .WriteProperty("eventType", ctx.eventType)
                .WriteProperty("eventConsumer", ctx.consumer));

            using (log.MeasureTrace(logContext, (ctx, w) => w
                .WriteProperty("action", "HandleEvent")
                .WriteProperty("actionId", ctx.eventId)
                .WriteProperty("status", "Completed")
                .WriteProperty("eventId", ctx.eventId)
                .WriteProperty("eventType", ctx.eventType)
                .WriteProperty("eventConsumer", ctx.consumer)))
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
                currentSubscription = CreateSubscription(eventConsumer.EventsFilter, position);
            }
            else
            {
                currentSubscription.WakeUp();
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

        protected virtual IEventConsumerGrain GetSelf()
        {
            return this.AsReference<IEventConsumerGrain>();
        }

        protected virtual IEventSubscription CreateSubscription(IEventStore store, IEventSubscriber subscriber, string streamFilter, string position)
        {
            return new RetrySubscription(store, subscriber, streamFilter, position);
        }

        private IEventSubscription CreateSubscription(string streamFilter, string position)
        {
            return CreateSubscription(eventStore, new WrapperSubscription(GetSelf(), scheduler), streamFilter, position);
        }
    }
}