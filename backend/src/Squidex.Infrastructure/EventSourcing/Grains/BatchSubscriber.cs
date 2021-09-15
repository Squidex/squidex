// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    internal sealed class BatchSubscriber : IEventSubscriber, IEventSubscription
    {
        private readonly IEventSubscription eventSubscription;
        private readonly Channel<object> taskQueue;
        private readonly Channel<EventSource> parseQueue;
        private readonly Task handleTask;
        private readonly CancellationTokenSource completed = new CancellationTokenSource();

        public object? Sender
        {
            get => eventSubscription.Sender!;
        }

        private sealed record EventSource(StoredEvent StoredEvent, object Sender);
        private sealed record BatchItem(Envelope<IEvent>? Event, string Position, object Sender);
        private sealed record BatchJob(BatchItem[] Items);
        private sealed record ErrorJob(Exception Exception, object? Sender);

        public BatchSubscriber(
            EventConsumerGrain grain,
            IEventDataFormatter eventDataFormatter,
            IEventConsumer eventConsumer,
            Func<IEventSubscriber, IEventSubscription> factory)
        {
            eventSubscription = factory(this);

            var batchSize = Math.Max(1, eventConsumer.BatchSize);
            var batchDelay = Math.Max(100, eventConsumer.BatchDelay);

            parseQueue = Channel.CreateBounded<EventSource>(new BoundedChannelOptions(batchSize)
            {
                AllowSynchronousContinuations = true,
                SingleReader = true,
                SingleWriter = true
            });

            taskQueue = Channel.CreateBounded<object>(new BoundedChannelOptions(2)
            {
                SingleReader = true,
                SingleWriter = true
            });

            var batchQueue = Channel.CreateBounded<object>(new BoundedChannelOptions(batchSize)
            {
                AllowSynchronousContinuations = true,
                SingleReader = true,
                SingleWriter = true
            });

#pragma warning disable MA0040 // Flow the cancellation token
            batchQueue.Batch<BatchItem, object>(taskQueue, x => new BatchJob(x.ToArray()), batchSize, batchDelay);

            Task.Run(async () =>
            {
                await foreach (var (storedEvent, sender) in parseQueue.Reader.ReadAllAsync(completed.Token))
                {
                    try
                    {
                        Envelope<IEvent>? @event = null;

                        if (eventConsumer.Handles(storedEvent))
                        {
                            @event = eventDataFormatter.ParseIfKnown(storedEvent);
                        }

                        await batchQueue.Writer.WriteAsync(new BatchItem(@event, storedEvent.EventPosition, sender), completed.Token);
                    }
                    catch (Exception ex)
                    {
                        await taskQueue.Writer.WriteAsync(new ErrorJob(ex, sender), completed.Token);
                    }
                }
            }).ContinueWith(x => batchQueue.Writer.TryComplete(x.Exception));
#pragma warning restore MA0040 // Flow the cancellation token

            handleTask = Run(grain);
        }

        private async Task Run(EventConsumerGrain grain)
        {
            try
            {
                await foreach (var task in taskQueue.Reader.ReadAllAsync(completed.Token))
                {
                    var sender = eventSubscription?.Sender;

                    if (sender == null)
                    {
                        continue;
                    }

                    switch (task)
                    {
                        case ErrorJob error when error.Exception is not OperationCanceledException:
                            {
                                if (ReferenceEquals(error.Sender, sender))
                                {
                                    await grain.OnErrorAsync(sender, error.Exception);
                                }

                                break;
                            }

                        case BatchJob batch:
                            {
                                foreach (var itemsBySender in batch.Items.GroupBy(x => x.Sender))
                                {
                                    if (ReferenceEquals(itemsBySender.Key, sender))
                                    {
                                        var position = itemsBySender.Last().Position;

                                        await grain.OnEventsAsync(sender, itemsBySender.Select(x => x.Event).NotNull().ToList(), position);
                                    }
                                }

                                break;
                            }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        public Task CompleteAsync()
        {
            parseQueue.Writer.TryComplete();

            return handleTask;
        }

        void IEventSubscription.Unsubscribe()
        {
            completed.Cancel();

            eventSubscription.Unsubscribe();
        }

        void IEventSubscription.WakeUp()
        {
            eventSubscription.WakeUp();
        }

        async Task IEventSubscriber.OnEventAsync(IEventSubscription subscription, StoredEvent storedEvent)
        {
            if (subscription.Sender != null)
            {
                await parseQueue.Writer.WriteAsync(new EventSource(storedEvent, subscription.Sender), completed.Token);
            }
        }

        async Task IEventSubscriber.OnErrorAsync(IEventSubscription subscription, Exception exception)
        {
            if (subscription.Sender != null && exception is not OperationCanceledException)
            {
                await taskQueue.Writer.WriteAsync(new ErrorJob(exception, subscription.Sender), completed.Token);
            }
        }
    }
}
