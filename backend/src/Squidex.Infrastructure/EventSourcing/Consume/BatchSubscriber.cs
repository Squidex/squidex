// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Channels;
using Squidex.Infrastructure.Tasks;

#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.EventSourcing.Consume
{
    internal sealed class BatchSubscriber : IEventSubscriber, IEventSubscription
    {
        private readonly IEventSubscription eventSubscription;
        private readonly Channel<object> taskQueue;
        private readonly Channel<EventSource> parseQueue;
        private readonly Task handleTask;
        private readonly CancellationTokenSource completed = new CancellationTokenSource();

        private sealed record EventSource(StoredEvent StoredEvent);
        private sealed record BatchItem(Envelope<IEvent>? Event, string Position);
        private sealed record BatchJob(BatchItem[] Items);
        private sealed record ErrorJob(Exception Exception);

        public BatchSubscriber(
            EventConsumerProcessor processor,
            IEventFormatter eventFormatter,
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
                await foreach (var source in parseQueue.Reader.ReadAllAsync(completed.Token))
                {
                    var storedEvent = source.StoredEvent;
                    try
                    {
                        Envelope<IEvent>? @event = null;

                        if (eventConsumer.Handles(storedEvent))
                        {
                            @event = eventFormatter.ParseIfKnown(storedEvent);
                        }

                        await batchQueue.Writer.WriteAsync(new BatchItem(@event, storedEvent.EventPosition), completed.Token);
                    }
                    catch (Exception ex)
                    {
                        await taskQueue.Writer.WriteAsync(new ErrorJob(ex), completed.Token);
                    }
                }
            }).ContinueWith(x => batchQueue.Writer.TryComplete(x.Exception));
#pragma warning restore MA0040 // Flow the cancellation token

            handleTask = Run(processor);
        }

        private async Task Run(EventConsumerProcessor processor)
        {
            try
            {
                await foreach (var task in taskQueue.Reader.ReadAllAsync(completed.Token))
                {
                    switch (task)
                    {
                        case ErrorJob error when error.Exception is not OperationCanceledException:
                            {
                                await processor.OnErrorAsync(this, error.Exception);
                                break;
                            }

                        case BatchJob batch:
                            {
                                var eventsPosition = batch.Items[^1].Position;
                                var eventsCollection = batch.Items.Select(x => x.Event).NotNull().ToList();

                                await processor.OnEventsAsync(this, eventsCollection, eventsPosition);
                                break;
                            }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                await processor.OnErrorAsync(this, ex);
            }
        }

        public Task CompleteAsync()
        {
            parseQueue.Writer.TryComplete();

            return handleTask;
        }

        public void Dispose()
        {
            completed.Cancel();

            eventSubscription.Dispose();
        }

        public void WakeUp()
        {
            eventSubscription.WakeUp();
        }

        ValueTask IEventSubscriber.OnEventAsync(IEventSubscription subscription, StoredEvent storedEvent)
        {
            return parseQueue.Writer.WriteAsync(new EventSource(storedEvent), completed.Token);
        }

        ValueTask IEventSubscriber.OnErrorAsync(IEventSubscription subscription, Exception exception)
        {
            return taskQueue.Writer.WriteAsync(new ErrorJob(exception), completed.Token);
        }
    }
}
