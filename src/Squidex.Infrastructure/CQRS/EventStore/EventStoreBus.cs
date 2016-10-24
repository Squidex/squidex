// ==========================================================================
//  EventStoreBus.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Logging;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Infrastructure.CQRS.EventStore
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
        private EventStoreCatchUpSubscription catchSubscription;
        private bool isLive;

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
        }

        public void Subscribe(string streamName = "$all")
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

            if (catchSubscription != null)
            {
                return;
            }

            var position = positions.ReadPosition();
            
            logger.LogInformation("Subscribing from {0}", position.HasValue ? position.Value.ToString() : "beginning");

            var settings =
                new CatchUpSubscriptionSettings(
                    int.MaxValue, 4096,
                    true,
                    true);

            catchSubscription = 
                connection.SubscribeToStreamFrom(streamName, position, settings,
                    OnEvent, 
                    OnLiveProcessingStarted, 
                    userCredentials: credentials);
        }

        private void OnEvent(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent)
        {
            try
            {
                if (resolvedEvent.OriginalEvent.EventStreamId.StartsWith("$", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (!liveConsumers.Any() && !catchConsumers.Any())
                {
                    return;
                }

                var @event = formatter.Parse(resolvedEvent);

                logger.LogInformation("Received event {0} ({1})", @event.Payload.GetType().Name, @event.Headers.AggregateId());

                if (isLive)
                {
                    DispatchConsumers(liveConsumers, @event).Wait();
                }

                DispatchConsumers(catchConsumers, @event).Wait();
            }
            finally
            {
                positions.WritePosition(resolvedEvent.OriginalEventNumber);
            }
        }

        private void OnLiveProcessingStarted(EventStoreCatchUpSubscription subscription)
        {
            isLive = true;
        }

        private Task DispatchConsumers(IEnumerable<IEventConsumer> consumers, Envelope<IEvent> @event)
        {
            return Task.WhenAll(consumers.Select(c => DispatchConsumer(@event, c)).ToList());
        }

        private async Task DispatchConsumer(Envelope<IEvent> @event, IEventConsumer consumer)
        {
            try
            {
                await consumer.On(@event);
            }
            catch (Exception ex)
            {
                var eventId = new EventId(10001, "EventConsumeFailed");

                logger.LogError(eventId, ex, "'{0}' failed to handle event {1} ({2})", consumer, @event.Payload, @event.Headers.EventId());
            }
        }
    }
}