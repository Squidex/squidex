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
using System.Reflection;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Logging;
using Squidex.Infrastructure.CQRS.Events;
// ReSharper disable InvertIf

namespace Squidex.Infrastructure.CQRS.EventStore
{
    public sealed class EventStoreBus : IDisposable
    {
        private readonly IEventStoreConnection connection;
        private readonly UserCredentials credentials;
        private readonly EventStoreFormatter formatter;
        private readonly IEnumerable<ILiveEventConsumer> liveConsumers;
        private readonly IEnumerable<ICatchEventConsumer> catchConsumers;
        private readonly ILogger<EventStoreBus> logger;
        private readonly IStreamPositionStorage positions;
        private readonly List<EventStoreCatchUpSubscription> catchSubscriptions = new List<EventStoreCatchUpSubscription>();
        private EventStoreSubscription liveSubscription;
        private string streamName;
        private bool isSubscribed;

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

        public void Dispose()
        {
            lock (catchSubscriptions)
            {
                foreach (var catchSubscription in catchSubscriptions)
                {
                    catchSubscription.Stop(TimeSpan.FromMinutes(1));
                }

                liveSubscription.Unsubscribe();
            }
        }

        public void Subscribe(string streamToConnect = "$all")
        {
            Guard.NotNullOrEmpty(streamToConnect, nameof(streamToConnect));

            if (isSubscribed)
            {
                return;
            }

            streamName = streamToConnect;

            SubscribeLive();
            SubscribeCatch();

            isSubscribed = true;
        }

        private void SubscribeLive()
        {
            Task.Run(async () =>
            {
                liveSubscription =
                    await connection.SubscribeToStreamAsync(streamName, true,
                        (subscription, resolvedEvent) =>
                        {
                            OnLiveEvent(resolvedEvent);
                        }, (subscription, dropped, ex) =>
                        {
                            OnConnectionDropped();
                        }, credentials);
            }).Wait();
        }

        private void OnConnectionDropped()
        {
            try
            {
                liveSubscription.Close();

                logger.LogError("Subscription closed");
            }
            finally
            {
                SubscribeLive();
            }
        }

        private void SubscribeCatch()
        {
            foreach (var catchConsumer in catchConsumers)
            {
                SubscribeCatchFor(catchConsumer);
            }
        }

        private void SubscribeCatchFor(IEventConsumer consumer)
        {
            var subscriptionName = consumer.GetType().GetTypeInfo().Name;

            var position = positions.ReadPosition(subscriptionName);

            logger.LogInformation("[{0}]: Subscribing from {0}", consumer, position ?? 0);

            var settings =
                new CatchUpSubscriptionSettings(
                    int.MaxValue, 4096,
                    true,
                    true);

            var catchSubscription =
                connection.SubscribeToStreamFrom(streamName, position, settings,
                    (subscription, resolvedEvent) =>
                    {
                        OnCatchEvent(consumer, resolvedEvent, subscriptionName, subscription);
                    }, userCredentials: credentials);

            lock (catchSubscriptions)
            {
                catchSubscriptions.Add(catchSubscription);
            }
        }

        private void OnLiveEvent(ResolvedEvent resolvedEvent)
        {
            Envelope<IEvent> @event = null;

            try
            {
                @event = formatter.Parse(new EventWrapper(resolvedEvent));
            }
            catch (Exception ex)
            {
                logger.LogError(InfrastructureErrors.EventDeserializationFailed, ex,
                    "[LiveConsumers]: Failed to deserialize event {0}#{1}", streamName,
                    resolvedEvent.OriginalEventNumber);
            }

            if (@event != null)
            {
                DispatchConsumers(liveConsumers, @event).Wait();
            }
        }

        private void OnCatchEvent(IEventConsumer consumer, ResolvedEvent resolvedEvent, string subscriptionName, EventStoreCatchUpSubscription subscription)
        {
            if (resolvedEvent.OriginalEvent.EventStreamId.StartsWith("$", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var isFailed = false;

            Envelope<IEvent> @event = null;

            try
            {
                @event = formatter.Parse(new EventWrapper(resolvedEvent));
            }
            catch (Exception ex)
            {
                logger.LogError(InfrastructureErrors.EventDeserializationFailed, ex,
                    "[{consumer}]: Failed to deserialize event {1}#{2}", consumer, streamName, 
                    resolvedEvent.OriginalEventNumber);

                isFailed = true;
            }

            if (@event != null)
            {
                try
                {
                    logger.LogInformation("Received event {0} ({1})", @event.Payload.GetType().Name, @event.Headers.AggregateId());

                    consumer.On(@event).Wait();

                    positions.WritePosition(subscriptionName, resolvedEvent.OriginalEventNumber);
                }
                catch (Exception ex)
                {
                    logger.LogError(InfrastructureErrors.EventHandlingFailed, ex,
                        "[{0}]: Failed to handle event {1} ({2})", consumer, 
                        @event.Payload, 
                        @event.Headers.EventId());
                }
            }

            if (isFailed)
            {
                lock (catchSubscriptions)
                {
                    subscription.Stop();

                    catchSubscriptions.Remove(subscription);
                }
            }
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
                logger.LogError(InfrastructureErrors.EventHandlingFailed, ex,
                    "[{0}]: Failed to handle event {1} ({2})", consumer, @event.Payload, @event.Headers.EventId());
            }
        }
    }
}