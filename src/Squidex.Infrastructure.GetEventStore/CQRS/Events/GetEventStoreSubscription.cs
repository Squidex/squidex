// ==========================================================================
//  GetEventStoreSubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.Projections;
using Squidex.Infrastructure.Actors;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
{
    internal sealed class GetEventStoreSubscription : Actor, IEventSubscription
    {
        private const int ReconnectWindowMax = 5;
        private const int ReconnectWaitMs = 1000;
        private static readonly TimeSpan TimeBetweenReconnects = TimeSpan.FromMinutes(5);
        private static readonly ConcurrentDictionary<string, bool> SubscriptionsCreated = new ConcurrentDictionary<string, bool>();
        private readonly IEventStoreConnection connection;
        private readonly IEventSubscriber subscriber;
        private readonly string prefix;
        private readonly string streamName;
        private readonly string streamFilter;
        private readonly string projectionHost;
        private readonly Queue<DateTime> reconnectTimes = new Queue<DateTime>();
        private EventStoreCatchUpSubscription subscription;
        private long? position;

        private sealed class ESConnect
        {
        }

        private abstract class ESMessage
        {
            public EventStoreCatchUpSubscription Subscription { get; set; }
        }

        private sealed class ESSubscriptionFailed : ESMessage
        {
            public Exception Exception { get; set; }
        }

        private sealed class ESEventReceived : ESMessage
        {
            public ResolvedEvent Event { get; set; }
        }

        public GetEventStoreSubscription(
            IEventStoreConnection connection,
            IEventSubscriber subscriber,
            string projectionHost,
            string prefix,
            string position,
            string streamFilter)
        {
            this.connection = connection;
            this.position = ParsePosition(position);
            this.prefix = prefix;
            this.projectionHost = projectionHost;
            this.streamFilter = streamFilter;
            this.subscriber = subscriber;

            streamName = ParseFilter(prefix, streamFilter);

            DispatchAsync(new ESConnect()).Forget();
        }

        public Task StopAsync()
        {
            return StopAndWaitAsync();
        }

        protected override Task OnStop()
        {
            subscription?.Stop();

            return TaskHelper.Done;
        }

        protected override async Task OnError(Exception exception)
        {
            await subscriber.OnErrorAsync(this, exception);

            await StopAsync();
        }

        protected override async Task OnMessage(object message)
        {
            switch (message)
            {
                case ESConnect connect when subscription == null:
                {
                    await InitializeAsync();

                    subscription = SubscribeToStream();

                    break;
                }

                case ESSubscriptionFailed subscriptionFailed when subscriptionFailed.Subscription == subscription:
                {
                    subscription.Stop();
                    subscription = null;

                    if (CanReconnect(DateTime.UtcNow))
                    {
                        Task.Delay(ReconnectWaitMs).ContinueWith(t => DispatchAsync(new ESConnect())).Forget();
                    }
                    else
                    {
                        throw subscriptionFailed.Exception;
                    }

                    break;
                }

                case ESEventReceived eventReceived when eventReceived.Subscription == subscription:
                {
                    var storedEvent = Formatter.Read(eventReceived.Event);

                    await subscriber.OnEventAsync(this, storedEvent);

                    position = eventReceived.Event.OriginalEventNumber;

                    break;
                }
            }
        }

        private EventStoreCatchUpSubscription SubscribeToStream()
        {
            var settings = CatchUpSubscriptionSettings.Default;

            return connection.SubscribeToStreamFrom(streamName, position, settings,
                (s, e) =>
                {
                    DispatchAsync(new ESEventReceived { Event = e, Subscription = s }).Forget();
                }, null,
                (s, reason, ex) =>
                {
                    if (reason == SubscriptionDropReason.ConnectionClosed ||
                        reason == SubscriptionDropReason.UserInitiated)
                    {
                        ex = ex ?? new ConnectionClosedException($"Subscription closed with reason {reason}.");

                        DispatchAsync(new ESSubscriptionFailed { Exception = ex, Subscription = s }).Forget();
                    }
                });
        }

        private bool CanReconnect(DateTime utcNow)
        {
            reconnectTimes.Enqueue(utcNow);

            while (reconnectTimes.Count >= ReconnectWindowMax)
            {
                reconnectTimes.Dequeue();
            }

            return reconnectTimes.Count < ReconnectWindowMax && (reconnectTimes.Count == 0 || (utcNow - reconnectTimes.Peek()) > TimeBetweenReconnects);
        }

        private async Task InitializeAsync()
        {
            if (SubscriptionsCreated.TryAdd(streamName, true))
            {
                var projectsManager = await ConnectToProjections();

                var projectionConfig =
                    $@"fromAll()
                        .when({{
                            $any: function (s, e) {{
                                if (e.streamId.indexOf('{prefix}') === 0 && /{streamFilter}/.test(e.streamId.substring({prefix.Length + 1}))) {{
                                    linkTo('{streamName}', e);
                                }}
                            }}
                        }});";

                try
                {
                    var credentials = connection.Settings.DefaultUserCredentials;

                    await projectsManager.CreateContinuousAsync($"${streamName}", projectionConfig, credentials);
                }
                catch (Exception ex)
                {
                    if (!ex.Is<ProjectionCommandConflictException>())
                    {
                        throw;
                    }
                }
            }
        }

        private static string ParseFilter(string prefix, string filter)
        {
            return $"by-{prefix.Simplify()}-{filter.Simplify()}";
        }

        private static long? ParsePosition(string position)
        {
            return long.TryParse(position, out var parsedPosition) ? (long?)parsedPosition : null;
        }

        private async Task<ProjectionsManager> ConnectToProjections()
        {
            var addressParts = projectionHost.Split(':');

            if (addressParts.Length < 2 || !int.TryParse(addressParts[1], out var port))
            {
                port = 2113;
            }

            var endpoints = await Dns.GetHostAddressesAsync(addressParts[0]);
            var endpoint = new IPEndPoint(endpoints.First(x => x.AddressFamily == AddressFamily.InterNetwork), port);

            var projectionsManager =
                new ProjectionsManager(
                    connection.Settings.Log, endpoint,
                    connection.Settings.OperationTimeout);

            return projectionsManager;
        }
    }
}
