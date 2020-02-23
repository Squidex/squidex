// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using Squidex.Infrastructure.Json;

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

            this.position = projectionClient.ParsePositionOrNull(position);
            this.prefix = prefix;

            var streamName = projectionClient.CreateProjectionAsync(streamFilter).Result;

            this.serializer = serializer;
            this.subscriber = subscriber;

            subscription = SubscribeToStream(streamName);
        }

        public Task StopAsync()
        {
            subscription.Stop();

            return Task.CompletedTask;
        }

        public void WakeUp()
        {
        }

        private EventStoreCatchUpSubscription SubscribeToStream(string streamName)
        {
            var settings = CatchUpSubscriptionSettings.Default;

            return connection.SubscribeToStreamFrom(streamName, position, settings,
                (s, e) =>
                {
                    var storedEvent = Formatter.Read(e, prefix, serializer);

                    subscriber.OnEventAsync(this, storedEvent).Wait();
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
