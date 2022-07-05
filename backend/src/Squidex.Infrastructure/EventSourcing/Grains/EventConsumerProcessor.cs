// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public class EventConsumerProcessor
    {
        private readonly SimpleState<EventConsumerState> state;
        private readonly IEventFormatter eventFormatter;
        private readonly IEventConsumer? eventConsumer;
        private readonly IEventStore eventStore;
        private readonly ILogger<EventConsumerProcessor> log;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private IEventSubscription? currentSubscription;

        public EventConsumerState State
        {
            get => state.Value;
            set => state.Value = value;
        }

        public EventConsumerProcessor(
            IPersistenceFactory<EventConsumerState> persistenceFactory,
            IEventConsumer eventConsumer,
            IEventFormatter eventFormatter,
            IEventStore eventStore,
            ILogger<EventConsumerProcessor> log)
        {
            this.eventStore = eventStore;
            this.eventFormatter = eventFormatter;
            this.eventConsumer = eventConsumer;
            this.log = log;

            state = new SimpleState<EventConsumerState>(persistenceFactory, GetType(), eventConsumer.Name);
        }

        public virtual Task InitializeAsync(
            CancellationToken ct)
        {
            return state.LoadAsync(ct);
        }

        public virtual async Task CompleteAsync()
        {
            if (currentSubscription is BatchSubscriber batchSubscriber)
            {
                try
                {
                    await batchSubscriber.CompleteAsync();
                }
                catch (Exception ex)
                {
                    log.LogCritical(ex, "Failed to complete consumer.");
                }
            }
        }

        public virtual Task OnEventsAsync(object sender, IReadOnlyList<Envelope<IEvent>> events, string position)
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

        public virtual Task OnErrorAsync(object sender, Exception exception)
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

        public virtual async Task ActivateAsync()
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

        public virtual async Task StartAsync()
        {
            if (!State.IsStopped)
            {
                return;
            }

            await DoAndUpdateStateAsync(() =>
            {
                Subscribe();

                State = State.Started();
            }, State.Position);
        }

        public virtual async Task StopAsync()
        {
            if (State.IsStopped)
            {
                return;
            }

            await DoAndUpdateStateAsync(() =>
            {
                Unsubscribe();

                State = State.Stopped();
            }, State.Position);
        }

        public virtual async Task ResetAsync()
        {
            await DoAndUpdateStateAsync(async () =>
            {
                Unsubscribe();

                await ClearAsync();

                State = EventConsumerState.Initial;

                Subscribe();
            }, State.Position);
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

                    log.LogCritical(ex, "Failed to update consumer {consumer} at position {position} from {caller}.",
                        eventConsumer!.Name, position, caller);

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
            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug("Event consumer {consumer} reset started", eventConsumer!.Name);
            }

            var watch = ValueStopwatch.StartNew();
            try
            {
                await eventConsumer!.ClearAsync();
            }
            finally
            {
                log.LogDebug("Event consumer {consumer} reset completed after {time}ms.", eventConsumer!.Name, watch.Stop());
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
            return new BatchSubscriber(this, eventFormatter, eventConsumer!, CreateRetrySubscription);
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
