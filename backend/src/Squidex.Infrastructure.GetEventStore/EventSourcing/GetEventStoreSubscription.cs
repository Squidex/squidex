// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using EventStore.Client;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.EventSourcing;

internal sealed class GetEventStoreSubscription : IEventSubscription
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private StreamSubscription subscription;

    public GetEventStoreSubscription(
        IEventSubscriber<StoredEvent> eventSubscriber,
        EventStoreClient client,
        EventStoreProjectionClient projectionClient,
        IJsonSerializer serializer,
        string? position,
        string? prefix,
        string? streamFilter)
    {
        Task.Run(async () =>
        {
            var ct = cts.Token;

            var streamName = await projectionClient.CreateProjectionAsync(streamFilter);

            async Task OnEvent(StreamSubscription subscription, ResolvedEvent @event,
                CancellationToken ct)
            {
                var storedEvent = Formatter.Read(@event, prefix, serializer);

                await eventSubscriber.OnNextAsync(this, storedEvent);
            }

            void OnError(StreamSubscription subscription, SubscriptionDroppedReason reason, Exception? ex)
            {
                if (reason != SubscriptionDroppedReason.Disposed &&
                    reason != SubscriptionDroppedReason.SubscriberError)
                {
                    ex ??= new InvalidOperationException($"Subscription closed with reason {reason}.");

                    eventSubscriber.OnErrorAsync(this, ex).AsTask().Forget();
                }
            }

            if (!string.IsNullOrWhiteSpace(position))
            {
                var from = FromStream.After(position.ToPosition(true));

                subscription = await client.SubscribeToStreamAsync(streamName, from,
                    OnEvent, true,
                    OnError,
                    cancellationToken: ct);
            }
            else
            {
                var from = FromStream.Start;

                subscription = await client.SubscribeToStreamAsync(streamName, from,
                    OnEvent, true,
                    OnError,
                    cancellationToken: ct);
            }
        }, cts.Token);
    }

    public void Dispose()
    {
        subscription?.Dispose();

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
