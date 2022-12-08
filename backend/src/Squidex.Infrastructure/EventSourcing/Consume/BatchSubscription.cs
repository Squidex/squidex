// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Channels;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.EventSourcing.Consume;

internal sealed class BatchSubscription : IEventSubscriber<ParsedEvent>, IEventSubscription
{
    private readonly IEventSubscription eventSubscription;
    private readonly Channel<object> taskQueue;
    private readonly Channel<object> batchQueue;
    private readonly Task handleTask;
    private readonly CancellationTokenSource completed = new CancellationTokenSource();

    public BatchSubscription(
        IEventConsumer eventConsumer,
        IEventSubscriber<ParsedEvents> eventSubscriber,
        EventSubscriptionSource<ParsedEvent> eventSource)
    {
        var batchSize = Math.Max(1, eventConsumer.BatchSize);
        var batchDelay = Math.Max(100, eventConsumer.BatchDelay);

        taskQueue = Channel.CreateBounded<object>(new BoundedChannelOptions(2)
        {
            SingleReader = true,
            SingleWriter = true
        });

        batchQueue = Channel.CreateBounded<object>(new BoundedChannelOptions(batchSize)
        {
            AllowSynchronousContinuations = true,
            SingleReader = true,
            SingleWriter = true
        });

        batchQueue.Batch<ParsedEvent>(taskQueue, batchSize, batchDelay, completed.Token);

        handleTask = Run(eventSubscriber);

        // Run last to subscribe after everything is configured.
        eventSubscription = eventSource(this);
    }

    private async Task Run(IEventSubscriber<ParsedEvents> eventSink)
    {
        try
        {
            var isStopped = false;

            await foreach (var task in taskQueue.Reader.ReadAllAsync(completed.Token))
            {
                switch (task)
                {
                    case Exception exception when exception is not OperationCanceledException:
                        {
                            if (!completed.IsCancellationRequested)
                            {
                                await eventSink.OnErrorAsync(this, exception);
                            }

                            isStopped = true;
                            break;
                        }

                    case List<ParsedEvent> batch:
                        {
                            if (!completed.IsCancellationRequested)
                            {
                                // Events can be null if the event consumer is not interested in the stored event.
                                var eventList = batch.Select(x => x.Event).NotNull().ToList();
                                var eventPosition = batch[^1].Position;

                                // Use a struct here to save a few allocations.
                                await eventSink.OnNextAsync(this, new ParsedEvents(eventList, eventPosition));
                            }

                            break;
                        }
                }

                if (isStopped)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            if (!completed.IsCancellationRequested)
            {
                await eventSink.OnErrorAsync(this, ex);
            }
        }
    }

    public void Dispose()
    {
        if (completed.IsCancellationRequested)
        {
            return;
        }

        // It is not necessary to dispose the cancellation token source.
        completed.Cancel();

        // We do not lock here, it is the responsibility of the source subscription to be thread safe.
        eventSubscription.Dispose();
    }

    public async ValueTask CompleteAsync()
    {
        await eventSubscription.CompleteAsync();

        batchQueue.Writer.TryComplete();

        await handleTask;
    }

    public void WakeUp()
    {
        eventSubscription.WakeUp();
    }

    async ValueTask IEventSubscriber<ParsedEvent>.OnErrorAsync(IEventSubscription subscription, Exception exception)
    {
        try
        {
            // Forward the exception from one task only, but bypass the batch.
            await taskQueue.Writer.WriteAsync(exception, completed.Token);
        }
        catch (OperationCanceledException)
        {
            // These exception are acceptable and happens when an exception has been thrown before.
        }
        catch (ChannelClosedException)
        {
        }
    }

    async ValueTask IEventSubscriber<ParsedEvent>.OnNextAsync(IEventSubscription subscription, ParsedEvent @event)
    {
        try
        {
            await batchQueue.Writer.WriteAsync(@event, completed.Token);
        }
        catch (OperationCanceledException)
        {
            // These exception are acceptable and happens when an exception has been thrown before.
        }
        catch (ChannelClosedException)
        {
        }
    }
}
