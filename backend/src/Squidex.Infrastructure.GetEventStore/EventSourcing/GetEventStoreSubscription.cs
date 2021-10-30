// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
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

                if (!string.IsNullOrWhiteSpace(position))
                {
                    var streamPosition = position.ToPosition();

                    subscription = await client.SubscribeToStreamAsync(streamName, streamPosition,
                        async (_, @event, _) =>
                        {
                            var storedEvent = Formatter.Read(@event, prefix, serializer);

                            await subscriber.OnEventAsync(this, storedEvent);
                        },
                        true,
                        (_, reason, ex) =>
                        {
                            if (reason != SubscriptionDroppedReason.Disposed &&
                                reason != SubscriptionDroppedReason.SubscriberError)
                            {
                                ex ??= new InvalidOperationException($"Subscription closed with reason {reason}.");

                                subscriber.OnErrorAsync(this, ex);
                            }
                        },
                        cancellationToken: ct);
                }
                else
                {
                    subscription = await client.SubscribeToStreamAsync(streamName,
                        async (_, @event, _) =>
                        {
                            var storedEvent = Formatter.Read(@event, prefix, serializer);

                            await subscriber.OnEventAsync(this, storedEvent);
                        },
                        true,
                        (_, reason, ex) =>
                        {
                            if (reason != SubscriptionDroppedReason.Disposed &&
                                reason != SubscriptionDroppedReason.SubscriberError)
                            {
                                ex ??= new InvalidOperationException($"Subscription closed with reason {reason}.");

                                subscriber.OnErrorAsync(this, ex);
                            }
                        },
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
