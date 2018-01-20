// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.Projections;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.EventSourcing
{
    internal sealed class GetEventStoreSubscription : IEventSubscription
    {
        private readonly IEventStoreConnection eventStoreConnection;
        private readonly IEventSubscriber eventSubscriber;
        private readonly EventStoreCatchUpSubscription subscription;
        private readonly long? position;

        public GetEventStoreSubscription(
            IEventStoreConnection eventStoreConnection,
            IEventSubscriber eventSubscriber,
            ProjectionsManager projectionsManager,
            string prefix,
            string position,
            string streamFilter)
        {
            Guard.NotNull(eventSubscriber, nameof(eventSubscriber));

            this.eventStoreConnection = eventStoreConnection;
            this.eventSubscriber = eventSubscriber;
            this.position = ProjectionHelper.ParsePositionOrNull(position);

            var streamName = eventStoreConnection.CreateProjectionAsync(projectionsManager, prefix, streamFilter).Result;

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

            return eventStoreConnection.SubscribeToStreamFrom(streamName, position, settings,
                (s, e) =>
                {
                    var storedEvent = Formatter.Read(e);

                    eventSubscriber.OnEventAsync(this, storedEvent).Wait();
                }, null,
                (s, reason, ex) =>
                {
                    if (reason != SubscriptionDropReason.ConnectionClosed &&
                        reason != SubscriptionDropReason.UserInitiated)
                    {
                        ex = ex ?? new ConnectionClosedException($"Subscription closed with reason {reason}.");

                        eventSubscriber.OnErrorAsync(this, ex);
                    }
                });
        }
    }
}
