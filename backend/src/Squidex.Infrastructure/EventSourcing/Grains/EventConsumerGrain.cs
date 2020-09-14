// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Orleans;
using Orleans.Concurrency;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public class EventConsumerGrain : GrainOfString, IEventConsumerGrain
    {
        private readonly EventConsumerFactory eventConsumerFactory;
        private readonly IGrainState<EventConsumerState> state;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly IEventStore eventStore;
        private readonly ISemanticLog log;
        private ITargetBlock<Job> pipelineStart;
        private IDataflowBlock pipelineEnd;
        private TaskScheduler? scheduler;
        private IEventSubscription? currentSubscription;
        private IEventConsumer? eventConsumer;

        private sealed class Job
        {
            public bool ShouldHandle { get; set; }

            public StoredEvent Stored { get; set; }

            public Exception? Exception { get; set; }

            public Envelope<IEvent>? Event { get; set; }

            public IEventSubscription Subscription { get; set; }
        }

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
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventDataFormatter, nameof(eventDataFormatter));
            Guard.NotNull(eventConsumerFactory, nameof(eventConsumerFactory));
            Guard.NotNull(state, nameof(state));
            Guard.NotNull(log, nameof(log));

            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.eventConsumerFactory = eventConsumerFactory;
            this.state = state;

            this.log = log;
        }

        protected override Task OnActivateAsync(string key)
        {
            scheduler = TaskScheduler.Current;

            eventConsumer = eventConsumerFactory(key);

            CreatePipeline();

            return Task.CompletedTask;
        }

        private void CreatePipeline()
        {
            var parse = new TransformBlock<Job, Job>(job =>
            {
                if (job.ShouldHandle)
                {
                    try
                    {
                        job.Event = ParseKnownEvent(job.Stored);
                    }
                    catch (Exception ex)
                    {
                        job.Exception = ex;
                    }
                }

                return job;
            }, new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 2,
                MaxDegreeOfParallelism = 1,
                MaxMessagesPerTask = 1
            });

            var batchSize = Math.Max(1, eventConsumer!.BatchSize);

            var buffer = AsyncHelper.CreateBatchBlock<Job>(batchSize, 500, new GroupingDataflowBlockOptions
            {
                BoundedCapacity = batchSize * 2
            });

            var handle = new ActionBlock<IList<Job>>(async jobs =>
            {
                var exception = jobs.FirstOrDefault(x => x.Exception != null)?.Exception;

                await DoAndUpdateStateAsync(async () =>
                {
                    if (exception != null)
                    {
                        throw exception;
                    }

                    var events = jobs.Where(x => x.Subscription == currentSubscription).NotNull(x => x.Event).ToArray();

                    await eventConsumer.On(events);

                    var position = jobs.Last().Stored.EventPosition;

                    State = State.Handled(position, jobs.Count);
                });
            },
            new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 2,
                MaxDegreeOfParallelism = 1,
                MaxMessagesPerTask = 1,
                TaskScheduler = GetScheduler()
            });

            parse.LinkTo(buffer, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            buffer.LinkTo(handle, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            pipelineStart = parse;
            pipelineEnd = handle;
        }

        public override Task OnDeactivateAsync()
        {
            pipelineStart.Complete();

            return pipelineEnd.Completion;
        }

        public Task<Immutable<EventConsumerInfo>> GetStateAsync()
        {
            return Task.FromResult(CreateInfo());
        }

        private Immutable<EventConsumerInfo> CreateInfo()
        {
            return State.ToInfo(eventConsumer!.Name).AsImmutable();
        }

        public Task OnEventAsync(Immutable<IEventSubscription> subscription, Immutable<StoredEvent> storedEvent)
        {
            if (subscription.Value != currentSubscription)
            {
                return Task.CompletedTask;
            }

            var job = new Job
            {
                ShouldHandle = eventConsumer!.Handles(storedEvent.Value),
                Stored = storedEvent.Value,
                Subscription = subscription.Value
            };

            return pipelineStart.SendAsync(job);
        }

        public Task OnErrorAsync(Immutable<IEventSubscription> subscription, Immutable<Exception> exception)
        {
            if (subscription.Value != currentSubscription)
            {
                return Task.CompletedTask;
            }

            return DoAndUpdateStateAsync(() =>
            {
                Unsubscribe();

                State = State.Stopped(exception.Value);
            });
        }

        public async Task ActivateAsync()
        {
            if (State.IsFailed)
            {
                await DoAndUpdateStateAsync(() =>
                {
                    Subscribe(State.Position);

                    State = State.Started();
                });
            }
            else if (!State.IsStopped)
            {
                Subscribe(State.Position);
            }
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

        private Task DoAndUpdateStateAsync(Action action, [CallerMemberName] string? caller = null)
        {
            return DoAndUpdateStateAsync(() => { action(); return Task.CompletedTask; }, caller);
        }

        private async Task DoAndUpdateStateAsync(Func<Task> action, [CallerMemberName] string? caller = null)
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
                    .WriteProperty("eventConsumer", eventConsumer!.Name));

                State = State.Stopped(ex);
            }

            if (State != previousState)
            {
                await state.WriteAsync();
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

        private void Subscribe(string? position)
        {
            if (currentSubscription == null)
            {
                currentSubscription = CreateSubscription(eventConsumer!.EventsFilter, position);
            }
            else
            {
                currentSubscription.WakeUp();
            }
        }

        private Envelope<IEvent>? ParseKnownEvent(StoredEvent storedEvent)
        {
            try
            {
                var @event = eventDataFormatter.Parse(storedEvent.Data);

                @event.SetEventPosition(storedEvent.EventPosition);
                @event.SetEventStreamNumber(storedEvent.EventStreamNumber);

                return @event;
            }
            catch (TypeNameNotFoundException)
            {
                log.LogDebug(w => w.WriteProperty("oldEventFound", storedEvent.Data.Type));

                return null;
            }
        }

        protected virtual TaskScheduler GetScheduler()
        {
            return scheduler!;
        }

        protected virtual IEventConsumerGrain GetSelf()
        {
            return this.AsReference<IEventConsumerGrain>();
        }

        protected virtual IEventSubscription CreateSubscription(IEventStore store, IEventSubscriber subscriber, string filter, string? position)
        {
            return new RetrySubscription(store, subscriber, filter, position);
        }

        private IEventSubscription CreateSubscription(string streamFilter, string? position)
        {
            return CreateSubscription(eventStore, new WrapperSubscription(GetSelf(), GetScheduler()), streamFilter, position);
        }
    }
}