// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using EventStore.Client;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure.EventSourcing;

internal sealed class GetEventStoreSubscription : IEventSubscription
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();

    public GetEventStoreSubscription(
        IEventSubscriber<StoredEvent> eventSubscriber,
        EventStoreClient client,
        EventStoreProjectionClient projectionClient,
        IJsonSerializer serializer,
        string? position,
        string? prefix,
        StreamFilter filter)
    {
        var ct = cts.Token;

#pragma warning disable MA0134 // Observe result of async calls
        Task.Run(async () =>
        {
            var streamName = await projectionClient.CreateProjectionAsync(filter);

            var start = FromStream.Start;
            if (!string.IsNullOrWhiteSpace(position))
            {
                start = FromStream.After(position.ToPosition(true));
            }

            await using var subscription = client.SubscribeToStream(streamName, start, true, cancellationToken: ct);
            try
            {
                await foreach (var message in subscription.Messages)
                {
                    if (message is StreamMessage.Event @event)
                    {
                        var storedEvent = Formatter.Read(@event.ResolvedEvent, prefix, serializer);

                        await eventSubscriber.OnNextAsync(this, storedEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                var inner = new InvalidOperationException($"Subscription closed.", ex);

                await eventSubscriber.OnErrorAsync(this, ex);
            }
        }, ct);
#pragma warning restore MA0134 // Observe result of async calls
    }

    public void Dispose()
    {
        cts.Cancel();
    }

    public ValueTask CompleteAsync()
    {
        return default;
    }

    public void WakeUp()
    {
    }
}
