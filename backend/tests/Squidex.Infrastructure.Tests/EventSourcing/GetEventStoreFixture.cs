// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Projections;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class GetEventStoreFixture : IDisposable
    {
        private readonly IEventStoreConnection connection;

        public GetEventStore EventStore { get; }

        public GetEventStoreFixture()
        {
            connection = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@localhost:1113; HeartBeatTimeout=500; MaxReconnections=-1");

            EventStore = new GetEventStore(connection, TestUtils.DefaultSerializer, "test", "localhost");
            EventStore.InitializeAsync().Wait();
        }

        public void Dispose()
        {
            CleanupAsync().Wait();

            connection.Dispose();
        }

        private async Task CleanupAsync()
        {
            var endpoints = await Dns.GetHostAddressesAsync("localhost");
            var endpoint = new IPEndPoint(endpoints.First(x => x.AddressFamily == AddressFamily.InterNetwork), 2113);

            var credentials = connection.Settings.DefaultUserCredentials;

            var projectionsManager =
                new ProjectionsManager(
                    connection.Settings.Log, endpoint,
                    connection.Settings.OperationTimeout);

            foreach (var projection in await projectionsManager.ListAllAsync(credentials))
            {
                var name = projection.Name;

                if (name.StartsWith("by-test", StringComparison.OrdinalIgnoreCase))
                {
                    await projectionsManager.DisableAsync(name, credentials);
                    await projectionsManager.DeleteAsync(name, true, credentials);
                }
            }
        }
    }
}
