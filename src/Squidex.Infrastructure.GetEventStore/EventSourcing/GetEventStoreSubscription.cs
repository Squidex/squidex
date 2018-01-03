// ==========================================================================
//  GetEventStoreSubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
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
            string projectionHost,
            string prefix,
            string position,
            string streamFilter)
        {
            Guard.NotNull(eventSubscriber, nameof(eventSubscriber));
            Guard.NotNullOrEmpty(streamFilter, nameof(streamFilter));

            this.eventStoreConnection = eventStoreConnection;
            this.eventSubscriber = eventSubscriber;
            this.position = ParsePosition(position);

            var streamName = eventStoreConnection.CreateProjectionAsync(projectionHost, prefix, streamFilter).Result;

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

        private static long? ParsePosition(string position)
        {
            return long.TryParse(position, out var parsedPosition) ? (long?)parsedPosition : null;
        }
    }
}
