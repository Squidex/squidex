// ==========================================================================
//  GetEventStoreSubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.Projections;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
{
    internal sealed class GetEventStoreSubscription : IEventSubscription
    {
        private const string ProjectionName = "by-{0}-{1}";
        private static readonly ConcurrentDictionary<string, bool> SubscriptionsCreated = new ConcurrentDictionary<string, bool>();
        private readonly IEventStoreConnection eventStoreConnection;
        private readonly IEventSubscriber eventSubscriber;
        private readonly string prefix;
        private readonly string streamFilter;
        private readonly string projectionHost;
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
            this.prefix = prefix;
            this.projectionHost = projectionHost;
            this.streamFilter = streamFilter;

            var streamName = ParseFilter(prefix, streamFilter);

            InitializeAsync(streamName).Wait();

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

        private async Task InitializeAsync(string streamName)
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
                    var credentials = eventStoreConnection.Settings.DefaultUserCredentials;

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
                    eventStoreConnection.Settings.Log, endpoint,
                    eventStoreConnection.Settings.OperationTimeout);

            return projectionsManager;
        }

        private static string ParseFilter(string prefix, string filter)
        {
            return string.Format(CultureInfo.InvariantCulture, ProjectionName, prefix.Simplify(), filter.Simplify());
        }

        private static long? ParsePosition(string position)
        {
            return long.TryParse(position, out var parsedPosition) ? (long?)parsedPosition : null;
        }
    }
}
