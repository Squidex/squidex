// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.EventSourcing
{
    internal sealed class GetEventStoreSubscription : IEventSubscription
    {
        private readonly IEventStoreConnection connection;
        private readonly IEventSubscriber subscriber;
        private readonly IJsonSerializer serializer;
        private readonly string? prefix;
        private readonly EventStoreCatchUpSubscription subscription;
        private readonly long? position;

        public GetEventStoreSubscription(
            IEventStoreConnection connection,
            IEventSubscriber subscriber,
            IJsonSerializer serializer,
            ProjectionClient projectionClient,
            string? position,
            string? prefix,
            string? streamFilter)
        {
            this.connection = connection;

            this.position = ProjectionClient.ParsePositionOrNull(position);
            this.prefix = prefix;

            var streamName = AsyncHelper.Sync(() => projectionClient.CreateProjectionAsync(streamFilter));

            this.serializer = serializer;
            this.subscriber = subscriber;

            subscription = SubscribeToStream(streamName);
        }

        public void Unsubscribe()
        {
            subscription.Stop();
        }

        private EventStoreCatchUpSubscription SubscribeToStream(string streamName)
        {
            var settings = CatchUpSubscriptionSettings.Default;

            return connection.SubscribeToStreamFrom(streamName, position, settings,
                async (s, e) =>
                {
                    var storedEvent = Formatter.Read(e, prefix, serializer);

                    await subscriber.OnEventAsync(this, storedEvent);
                }, null,
                (s, reason, ex) =>
                {
                    if (reason != SubscriptionDropReason.ConnectionClosed &&
                        reason != SubscriptionDropReason.UserInitiated)
                    {
                        ex ??= new ConnectionClosedException($"Subscription closed with reason {reason}.");

                        subscriber.OnErrorAsync(this, ex);
                    }
                });
        }
    }
}
