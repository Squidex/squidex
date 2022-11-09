// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Channels;

namespace Squidex.Infrastructure.EventSourcing.Consume;

internal sealed class ParseSubscription : IEventSubscriber<StoredEvent>, IEventSubscription
{
    private readonly Channel<object> deserializeQueue;
    private readonly CancellationTokenSource completed = new CancellationTokenSource();
    private readonly Task deserializeTask;
    private readonly IEventSubscription eventSubscription;

    public ParseSubscription(
        IEventConsumer eventConsumer,
        IEventFormatter eventFormatter,
        IEventSubscriber<ParsedEvent> eventSubscriber,
        EventSubscriptionSource<StoredEvent> eventSource)
    {
        deserializeQueue = Channel.CreateBounded<object>(new BoundedChannelOptions(2)
        {
            AllowSynchronousContinuations = true,
            SingleReader = true,
            SingleWriter = true
        });

#pragma warning disable MA0040 // Flow the cancellation token
        deserializeTask = Task.Run(async () =>
        {
            try
            {
                var isFailed = false;

                await foreach (var input in deserializeQueue.Reader.ReadAllAsync(completed.Token))
                {
                    switch (input)
                    {
                        case Exception exception:
                            {
                                // Not very likely that the task is cancelled.
                                await eventSubscriber.OnErrorAsync(this, exception);

                                isFailed = true;
                                break;
                            }

                        case StoredEvent storedEvent:
                            {
                                Envelope<IEvent>? @event = null;

                                if (eventConsumer.Handles(storedEvent))
                                {
                                    @event = eventFormatter.ParseIfKnown(storedEvent);
                                }

                                // Parsing takes a little bit of time, so the task might have been cancelled.
                                if (!completed.IsCancellationRequested)
                                {
                                    // Also invoke the subscriber if the event is null to update the position.
                                    await eventSubscriber.OnNextAsync(this, new ParsedEvent(@event, storedEvent.EventPosition));
                                }

                                break;
                            }
                    }

                    if (isFailed)
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
                    await eventSubscriber.OnErrorAsync(this, ex);
                }
            }
        }).ContinueWith(x => deserializeQueue.Writer.TryComplete(x.Exception));

        // Run last to subscribe after everything is configured.
        eventSubscription = eventSource(this);
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

        deserializeQueue.Writer.TryComplete();

        await deserializeTask;
    }

    public void WakeUp()
    {
        eventSubscription.WakeUp();
    }

    async ValueTask IEventSubscriber<StoredEvent>.OnErrorAsync(IEventSubscription subscription, Exception exception)
    {
        try
        {
            // Forward the exception from one task only.
            await deserializeQueue.Writer.WriteAsync(exception, completed.Token);
        }
        catch (OperationCanceledException)
        {
            // These exception are acceptable and happens when an exception has been thrown before.
        }
        catch (ChannelClosedException)
        {
        }
    }

    async ValueTask IEventSubscriber<StoredEvent>.OnNextAsync(IEventSubscription subscription, StoredEvent @event)
    {
        try
        {
            await deserializeQueue.Writer.WriteAsync(@event, completed.Token);
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
