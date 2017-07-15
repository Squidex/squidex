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
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.Projections;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Infrastructure.GetEventStore
{
    internal sealed class EventStoreSubscription : IEventSubscription
    {
        private static readonly ConcurrentDictionary<string, bool> subscriptionsCreated = new ConcurrentDictionary<string, bool>();
        private readonly IEventStoreConnection connection;
        private readonly string position;
        private readonly string streamFilter;
        private readonly string streamName;
        private readonly string prefix;
        private readonly string projectionHost;
        private EventStoreCatchUpSubscription internalSubscription;

        public EventStoreSubscription(IEventStoreConnection connection, string streamFilter, string position, string prefix, string projectionHost)
        {
            this.prefix = prefix;
            this.position = position;
            this.connection = connection;
            this.streamFilter = streamFilter;
            this.projectionHost = projectionHost;

            streamName = CreateStreamName(streamFilter, prefix);
        }
       
        public void Dispose()
        {
            internalSubscription?.Stop();
        }

        public async Task SubscribeAsync(Func<StoredEvent, Task> handler)
        {
            Guard.NotNull(handler, nameof(handler));

            if (internalSubscription != null)
            {
                throw new InvalidOperationException("An handler has already been registered.");
            }

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

            long? eventStorePosition = null;

            if (long.TryParse(position, out var parsedPosition))
            {
                eventStorePosition = parsedPosition;
            }

            internalSubscription = connection.SubscribeToStreamFrom(streamName, eventStorePosition, CatchUpSubscriptionSettings.Default, (subscription, resolved) =>
            {
                var eventData = Formatter.Read(resolved.Event);

                handler(new StoredEvent(resolved.OriginalEventNumber.ToString(), resolved.Event.EventNumber, eventData)).Wait();
            });
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

            var projectsManager =
                new ProjectionsManager(
                    connection.Settings.Log, endpoint,
                    connection.Settings.OperationTimeout);
            return projectsManager;
        }

        private static string CreateStreamName(string streamFilter, string prefix)
        {
            var sb = new StringBuilder();

            sb.Append("by-");
            sb.Append(prefix.Trim(' ', '-'));
            sb.Append("-");

            var prevIsLetterOrDigit = false;

            foreach (var c in streamFilter)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(char.ToLowerInvariant(c));

                    prevIsLetterOrDigit = true;
                }
                else if (prevIsLetterOrDigit)
                {
                    sb.Append("-");

                    prevIsLetterOrDigit = false;
                }
            }

            return sb.ToString().Trim(' ', '-');
        }
    }
}
