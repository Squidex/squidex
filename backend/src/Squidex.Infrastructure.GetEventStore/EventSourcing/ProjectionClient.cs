// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.Projections;
using Squidex.Hosting.Configuration;
using Squidex.Text;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class ProjectionClient
    {
        private readonly ConcurrentDictionary<string, bool> projections = new ConcurrentDictionary<string, bool>();
        private readonly IEventStoreConnection connection;
        private readonly string projectionPrefix;
        private readonly string projectionHost;
        private ProjectionsManager projectionsManager;

        public ProjectionClient(IEventStoreConnection connection, string projectionPrefix, string projectionHost)
        {
            this.connection = connection;

            this.projectionPrefix = projectionPrefix;
            this.projectionHost = projectionHost;
        }

        private string CreateFilterProjectionName(string filter)
        {
            return $"by-{projectionPrefix.Slugify()}-{filter.Slugify()}";
        }

        public async Task<string> CreateProjectionAsync(string? streamFilter = null)
        {
            streamFilter ??= ".*";

            var name = CreateFilterProjectionName(streamFilter);

            var query =
                $@"fromAll()
                    .when({{
                        $any: function (s, e) {{
                            if (e.streamId.indexOf('{projectionPrefix}') === 0 && /{streamFilter}/.test(e.streamId.substring({projectionPrefix.Length + 1}))) {{
                                linkTo('{name}', e);
                            }}
                        }}
                    }});";

            await CreateProjectionAsync(name, query);

            return name;
        }

        private async Task CreateProjectionAsync(string name, string query)
        {
            if (projections.TryAdd(name, true))
            {
                try
                {
                    var credentials = connection.Settings.DefaultUserCredentials;

                    await projectionsManager.CreateContinuousAsync(name, query, credentials);
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

        public async Task ConnectAsync()
        {
            var addressParts = projectionHost.Split(':');

            if (addressParts.Length < 2 || !int.TryParse(addressParts[1], out var port))
            {
                port = 2113;
            }

            var endpoints = await Dns.GetHostAddressesAsync(addressParts[0]);
            var endpoint = new IPEndPoint(endpoints.First(x => x.AddressFamily == AddressFamily.InterNetwork), port);

            async Task ConnectToSchemaAsync(string schema)
            {
                projectionsManager =
                    new ProjectionsManager(
                        connection.Settings.Log, endpoint,
                        connection.Settings.OperationTimeout,
                        null,
                        schema);

                await projectionsManager.ListAllAsync(connection.Settings.DefaultUserCredentials);
            }

            try
            {
                try
                {
                    await ConnectToSchemaAsync("https");
                }
                catch (HttpRequestException)
                {
                    await ConnectToSchemaAsync("http");
                }
                catch (AggregateException ex) when (ex.Flatten().InnerException is HttpRequestException)
                {
                    await ConnectToSchemaAsync("http");
                }
            }
            catch (Exception ex)
            {
                var error = new ConfigurationError($"GetEventStore cannot connect to event store projections: {projectionHost}.");

                throw new ConfigurationException(error, ex);
            }
        }

        public static long? ParsePositionOrNull(string? position)
        {
            return long.TryParse(position, out var parsedPosition) ? (long?)parsedPosition : null;
        }

        public static long ParsePosition(string? position)
        {
            return long.TryParse(position, out var parsedPosition) ? parsedPosition + 1 : StreamPosition.Start;
        }
    }
}
