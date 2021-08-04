// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Tasks;
using Squidex.Log;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public class EventConsumerGrain : GrainOfString, IEventConsumerGrain
    {
        private readonly EventConsumerFactory eventConsumerFactory;
        private readonly IGrainState<EventConsumerState> state;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly IEventStore eventStore;
        private readonly ISemanticLog log;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private IEventSubscription? currentSubscription;
        private IEventConsumer? eventConsumer;

        private EventConsumerState State
        {
            get => state.Value;
            set => state.Value = value;
        }

        public EventConsumerGrain(
            EventConsumerFactory eventConsumerFactory,
            IGrainState<EventConsumerState> state,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            ISemanticLog log)
        {
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.eventConsumerFactory = eventConsumerFactory;
            this.state = state;

            this.log = log;
        }

        protected override Task OnActivateAsync(string key)
        {
            eventConsumer = eventConsumerFactory(key);

            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
            CompleteAsync().Forget();

            return Task.CompletedTask;
        }

        public async Task CompleteAsync()
        {
            if (currentSubscription is BatchSubscriber batchSubscriber)
            {
                await batchSubscriber.CompleteAsync();
            }
        }

        public Task<EventConsumerInfo> GetStateAsync()
        {
            return Task.FromResult(CreateInfo());
        }

        private EventConsumerInfo CreateInfo()
        {
            return State.ToInfo(eventConsumer!.Name);
        }

        public Task OnEventsAsync(object sender, IReadOnlyList<Envelope<IEvent>> events, string position)
        {
            if (!ReferenceEquals(sender, currentSubscription?.Sender))
            {
                return Task.CompletedTask;
            }

            return DoAndUpdateStateAsync(async () =>
            {
                await DispatchAsync(events);

                State = State.Handled(position, events.Count);
            }, State.Position);
        }

        public Task OnErrorAsync(object sender, Exception exception)
        {
            if (!ReferenceEquals(sender, currentSubscription?.Sender))
            {
                return Task.CompletedTask;
            }

            return DoAndUpdateStateAsync(() =>
            {
                Unsubscribe();

                State = State.Stopped(exception);
            }, State.Position);
        }

        public async Task ActivateAsync()
        {
            if (State.IsFailed)
            {
                await DoAndUpdateStateAsync(() =>
                {
                    Subscribe();

                    State = State.Started();
                }, State.Position);
            }
            else if (!State.IsStopped)
            {
                Subscribe();
            }
        }

        public async Task<EventConsumerInfo> StartAsync()
        {
            if (!State.IsStopped)
            {
                return CreateInfo();
            }

            await DoAndUpdateStateAsync(() =>
            {
                Subscribe();

                State = State.Started();
            }, State.Position);

            return CreateInfo();
        }

        public async Task<EventConsumerInfo> StopAsync()
        {
            if (State.IsStopped)
            {
                return CreateInfo();
            }

            await DoAndUpdateStateAsync(() =>
            {
                Unsubscribe();

                State = State.Stopped();
            }, State.Position);

            return CreateInfo();
        }

        public async Task<EventConsumerInfo> ResetAsync()
        {
            await DoAndUpdateStateAsync(async () =>
            {
                Unsubscribe();

                await ClearAsync();

                State = EventConsumerState.Initial;

                Subscribe();
            }, State.Position);

            return CreateInfo();
        }

        private async Task DispatchAsync(IReadOnlyList<Envelope<IEvent>> events)
        {
            if (events.Count > 0)
            {
                await eventConsumer!.On(events);
            }
        }

        private Task DoAndUpdateStateAsync(Action action, string? position, [CallerMemberName] string? caller = null)
        {
            return DoAndUpdateStateAsync(() =>
            {
                action();

                return Task.CompletedTask;
            }, position, caller);
        }

        private async Task DoAndUpdateStateAsync(Func<Task> action, string? position, [CallerMemberName] string? caller = null)
        {
            await semaphore.WaitAsync();
            try
            {
                var previousState = State;

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
                        .WriteProperty("eventPosition", position)
                        .WriteProperty("eventConsumer", eventConsumer!.Name));

                    State = previousState.Stopped(ex);
                }

                if (State != previousState)
                {
                    await state.WriteAsync();
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task ClearAsync()
        {
            var logContext = (actionId: Guid.NewGuid().ToString(), consumer: eventConsumer!.Name);

            log.LogDebug(logContext, (ctx, w) => w
                .WriteProperty("action", "EventConsumerReset")
                .WriteProperty("actionId", ctx.actionId)
                .WriteProperty("status", "Started")
                .WriteProperty("eventConsumer", ctx.consumer));

            using (log.MeasureInformation(logContext, (ctx, w) => w
                .WriteProperty("action", "EventConsumerReset")
                .WriteProperty("actionId", ctx.actionId)
                .WriteProperty("status", "Completed")
                .WriteProperty("eventConsumer", ctx.consumer)))
            {
                await eventConsumer.ClearAsync();
            }
        }

        private void Unsubscribe()
        {
            var subscription = Interlocked.Exchange(ref currentSubscription, null);

            subscription?.Unsubscribe();
        }

        private void Subscribe()
        {
            if (currentSubscription == null)
            {
                currentSubscription = CreateSubscription();
            }
            else
            {
                currentSubscription.WakeUp();
            }
        }

        private BatchSubscriber CreateSubscription()
        {
            return new BatchSubscriber(this, eventDataFormatter, eventConsumer!, CreateRetrySubscription);
        }

        protected virtual IEventSubscription CreateRetrySubscription(IEventSubscriber subscriber)
        {
            return new RetrySubscription(subscriber, CreateSubscription);
        }

        protected virtual IEventSubscription CreateSubscription(IEventSubscriber subscriber)
        {
            return eventStore.CreateSubscription(subscriber, eventConsumer!.EventsFilter, State.Position);
        }
    }
}
