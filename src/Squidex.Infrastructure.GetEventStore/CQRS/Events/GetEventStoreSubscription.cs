// ==========================================================================
//  GetEventStoreSubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.Projections;

// ReSharper disable InvertIf

namespace Squidex.Infrastructure.CQRS.Events
{
    internal sealed class EventStoreSubscription : DisposableObjectBase, IEventSubscription
    {
        private static readonly ConcurrentDictionary<string, bool> subscriptionsCreated = new ConcurrentDictionary<string, bool>();
        private readonly IEventStoreConnection connection;
        private readonly string position;
        private readonly string streamFilter;
        private readonly string streamName;
        private readonly string prefix;
        private readonly string projectionHost;
        private EventStoreCatchUpSubscription internalSubscription;

        public bool IsDropped { get; private set; }

        public EventStoreSubscription(IEventStoreConnection connection, string streamFilter, string position, string prefix, string projectionHost)
        {
            this.prefix = prefix;
            this.position = position;
            this.connection = connection;
            this.streamFilter = streamFilter;
            this.projectionHost = projectionHost;

            streamName = $"by-{prefix.Simplify()}-{streamFilter.Simplify()}";
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                internalSubscription?.Stop();
            }
        }

        public async Task SubscribeAsync(Func<StoredEvent, Task> onNext, Func<Exception, Task> onError = null)
        {
            Guard.NotNull(onNext, nameof(onNext));

            if (internalSubscription != null)
            {
                throw new InvalidOperationException("An handler has already been registered.");
            }

            await CreateProjectionAsync();

            long? eventStorePosition = null;

            if (long.TryParse(position, out var parsedPosition))
            {
                eventStorePosition = parsedPosition;
            }

            internalSubscription = connection.SubscribeToStreamFrom(streamName, eventStorePosition, CatchUpSubscriptionSettings.Default, 
                (subscription, resolved) =>
                {
                    var storedEvent = Formatter.Read(resolved);

                    onNext(storedEvent).Wait();
                }, subscriptionDropped: (subscription, reason, ex) =>
                {
                    if (reason != SubscriptionDropReason.UserInitiated &&
                        reason != SubscriptionDropReason.ConnectionClosed)
                    {
                        var exception = ex ?? new ConnectionClosedException($"Subscription closed with reason {reason}.");

                        onError?.Invoke(exception);
                    }
                    else
                    {
                        IsDropped = true;
                    }
                });
        }

        private async Task CreateProjectionAsync()
        {
            if (subscriptionsCreated.TryAdd(streamName, true))
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
                    await projectsManager.CreateContinuousAsync($"${streamName}", projectionConfig, connection.Settings.DefaultUserCredentials);
                }
                catch (ProjectionCommandConflictException)
                {
                    // Projection already exists.
                }
            }
        }

        private async Task<ProjectionsManager> ConnectToProjections()
        {
            var addressParts = projectionHost.Split(':');

            if (addressParts.Length < 2 || !int.TryParse(addressParts[1], out int port))
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
