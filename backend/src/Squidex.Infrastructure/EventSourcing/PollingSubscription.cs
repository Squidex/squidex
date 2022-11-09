// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Timers;

namespace Squidex.Infrastructure.EventSourcing;

public sealed class PollingSubscription : IEventSubscription
{
    private readonly CompletionTimer timer;

    public PollingSubscription(
        IEventStore eventStore,
        IEventSubscriber<StoredEvent> eventSubscriber,
        string? streamFilter,
        string? position)
    {
        timer = new CompletionTimer(5000, async ct =>
        {
            try
            {
                await foreach (var storedEvent in eventStore.QueryAllAsync(streamFilter, position, ct: ct))
                {
                    await eventSubscriber.OnNextAsync(this, storedEvent);

                    position = storedEvent.EventPosition;
                }
            }
            catch (Exception ex)
            {
                await eventSubscriber.OnErrorAsync(this, ex);
            }
        });
    }

    public ValueTask CompleteAsync()
    {
        return new ValueTask(timer.StopAsync());
    }

    public void Dispose()
    {
        timer.StopAsync().Forget();
    }

    public void WakeUp()
    {
        timer.SkipCurrentDelay();
    }
}
