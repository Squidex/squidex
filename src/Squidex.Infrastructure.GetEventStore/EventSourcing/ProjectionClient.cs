// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class ProjectionClient
    {
        private readonly ConcurrentDictionary<string, bool> projections = new ConcurrentDictionary<string, bool>();
        private readonly IEventStoreConnection connection;
        private readonly string prefix;
        private readonly string projectionHost;
        private ProjectionsManager projectionsManager;

        public ProjectionClient(IEventStoreConnection connection, string prefix, string projectionHost)
        {
            this.connection = connection;

            this.prefix = prefix;
            this.projectionHost = projectionHost;
        }

        private string CreateFilterProjectionName(string filter)
        {
            return $"by-{prefix.Slugify()}-{filter.Slugify()}";
        }

        private string CreatePropertyProjectionName(string property)
        {
            return $"by-{prefix.Slugify()}-{property.Slugify()}-property";
        }

        public async Task<string> CreateProjectionAsync(string property, object value)
        {
            var name = CreatePropertyProjectionName(property);

            var query =
                $@"fromAll()
                    .when({{
                        $any: function (s, e) {{
                            if (e.streamId.indexOf('{prefix}') === 0 && e.metadata.{property}) {{
                                linkTo('{name}-' + e.metadata.{property}, e);
                            }}
                        }}
                    }});";

            await CreateProjectionAsync(name, query);

            return $"{name}-{value}";
        }

        public async Task<string> CreateProjectionAsync(string streamFilter = null)
        {
            streamFilter = streamFilter ?? ".*";

            var name = CreateFilterProjectionName(streamFilter);

            var query =
                $@"fromAll()
                    .when({{
                        $any: function (s, e) {{
                            if (e.streamId.indexOf('{prefix}') === 0 && /{streamFilter}/.test(e.streamId.substring({prefix.Length + 1}))) {{
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

            projectionsManager =
                new ProjectionsManager(
                    connection.Settings.Log, endpoint,
                    connection.Settings.OperationTimeout);
            try
            {
                await projectionsManager.ListAllAsync(connection.Settings.DefaultUserCredentials);
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Cannot connect to event store projections: {projectionHost}.", ex);
            }
        }

        public long? ParsePositionOrNull(string position)
        {
            return long.TryParse(position, out var parsedPosition) ? (long?)parsedPosition : null;
        }

        public long ParsePosition(string position)
        {
            return long.TryParse(position, out var parsedPosition) ? parsedPosition : 0;
        }
    }
}
