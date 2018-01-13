// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.EventSourcing
{
    internal sealed class GetEventStoreSubscription : IEventSubscription
    {
        private readonly IEventStoreConnection connection;
        private readonly IEventSubscriber subscriber;
        private readonly EventStoreCatchUpSubscription subscription;
        private readonly long? position;

        public GetEventStoreSubscription(
            IEventStoreConnection connection,
            IEventSubscriber subscriber,
            ProjectionClient projectionClient,
            string prefix,
            string position,
            string streamFilter)
        {
            Guard.NotNull(subscriber, nameof(subscriber));

            this.connection = connection;
            this.position = projectionClient.ParsePositionOrNull(position);
            this.subscriber = subscriber;

            var streamName = projectionClient.CreateProjectionAsync(streamFilter).Result;

            subscription = SubscribeToStream(streamName);
        }

        public Task StopAsync()
        {
            subscription.Stop();

            return TaskHelper.Done;
        }

        private EventStoreCatchUpSubscription SubscribeToStream(string streamName)
        {
            var settings = CatchUpSubscriptionSettings.Default;

            return connection.SubscribeToStreamFrom(streamName, position, settings,
                (s, e) =>
                {
                    var storedEvent = Formatter.Read(e);

                    subscriber.OnEventAsync(this, storedEvent).Wait();
                }, null,
                (s, reason, ex) =>
                {
                    if (reason != SubscriptionDropReason.ConnectionClosed &&
                        reason != SubscriptionDropReason.UserInitiated)
                    {
                        ex = ex ?? new ConnectionClosedException($"Subscription closed with reason {reason}.");

                        subscriber.OnErrorAsync(this, ex);
                    }
                });
        }
    }
}
