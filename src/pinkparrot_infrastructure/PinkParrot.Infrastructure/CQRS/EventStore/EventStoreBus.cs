// ==========================================================================
//  EventStoreBus.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Logging;
using PinkParrot.Infrastructure.CQRS.Events;

namespace PinkParrot.Infrastructure.CQRS.EventStore
{
    public sealed class EventStoreBus
    {
        private readonly IEventStoreConnection connection;
        private readonly UserCredentials credentials;
        private readonly EventStoreFormatter formatter;
        private readonly IEnumerable<ILiveEventConsumer> liveConsumers;
        private readonly IEnumerable<ICatchEventConsumer> catchConsumers;
        private readonly ILogger<EventStoreBus> logger;
        private readonly IStreamPositionStorage positions;
        private EventStoreAllCatchUpSubscription catchSubscription;

        public EventStoreBus(
            ILogger<EventStoreBus> logger,
            IEnumerable<ILiveEventConsumer> liveConsumers,
            IEnumerable<ICatchEventConsumer> catchConsumers,
            IStreamPositionStorage positions,
            IEventStoreConnection connection,
            UserCredentials credentials,
            EventStoreFormatter formatter)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(positions, nameof(positions));
            Guard.NotNull(connection, nameof(connection));
            Guard.NotNull(credentials, nameof(credentials));
            Guard.NotNull(liveConsumers, nameof(liveConsumers));
            Guard.NotNull(catchConsumers, nameof(catchConsumers));

            this.logger = logger;
            this.formatter = formatter;
            this.positions = positions;
            this.connection = connection;
            this.credentials = credentials;
            this.liveConsumers = liveConsumers;
            this.catchConsumers = catchConsumers;

            Subscribe();
        }

        private void Subscribe()
        {
            var position = positions.ReadPosition();

            var now = DateTime.UtcNow;

            logger.LogInformation($"Subscribing from: {0}", position);

            var settings =
                new CatchUpSubscriptionSettings(
                    int.MaxValue, 4096,
                    true,
                    true);

            catchSubscription = connection.SubscribeToAllFrom(position, settings, (s, resolvedEvent) =>
            {
                var requireUpdate = false;

                Debug.WriteLine($"Last Position: {catchSubscription.LastProcessedPosition}");
                try
                {
                    if (resolvedEvent.OriginalEvent.EventStreamId.StartsWith("$", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    if (liveConsumers.Any() || catchConsumers.Any())
                    {
                        requireUpdate = true;

                        var @event = formatter.Parse(resolvedEvent);

                        if (resolvedEvent.Event.Created > now)
                        {
                            Dispatch(liveConsumers, @event);
                        }

                        Dispatch(catchConsumers, @event);
                    }

                    requireUpdate = requireUpdate || catchSubscription.LastProcessedPosition.CommitPosition % 2 == 0;
                }
                finally
                {
                    if (requireUpdate)
                    {
                        positions.WritePosition(catchSubscription.LastProcessedPosition);
                    }
                }
            }, userCredentials: credentials);
        }

        private void Dispatch(IEnumerable<IEventConsumer> consumers, Envelope<IEvent> @event)
        {
            foreach (var consumer in consumers)
            {
                try
                {
                    consumer.On(@event);
                }
                catch (Exception ex)
                {
                    var eventId = new EventId(10001, "EventConsumeFailed");

                    logger.LogError(eventId, ex, "'{0}' failed to handle event {1} ({2})", consumer, @event.Payload, @event.Headers.EventId());
                }
            }
        }
    }
}