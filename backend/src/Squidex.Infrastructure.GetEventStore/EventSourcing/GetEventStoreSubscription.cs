// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using EventStore.Client;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure.EventSourcing
{
    internal sealed class GetEventStoreSubscription : IEventSubscription
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private StreamSubscription subscription;

        public GetEventStoreSubscription(
            IEventSubscriber subscriber,
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

                    await subscriber.OnEventAsync(this, storedEvent);
                }

                void OnError(StreamSubscription subscription, SubscriptionDroppedReason reason, Exception? ex)
                {
                    if (reason != SubscriptionDroppedReason.Disposed &&
                        reason != SubscriptionDroppedReason.SubscriberError)
                    {
                        ex ??= new InvalidOperationException($"Subscription closed with reason {reason}.");

                        subscriber.OnErrorAsync(this, ex);
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

        public void Unsubscribe()
        {
            subscription?.Dispose();

            cts.Cancel();
        }
    }
}
