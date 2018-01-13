// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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

        private string CreateFilterStreamName(string filter)
        {
            return $"by-{StreamByFilter}-{prefix.Simplify()}-{filter.Simplify()}";
        }

        private string CreatePropertyStreamName(string property)
        {
            return $"by-{StreamByFilter}-{prefix.Simplify()}-{property.Simplify()}-property";
        }

        public async Task<string> CreateProjectionAsync(string property, object value)
        {
            var streamName = CreatePropertyStreamName(property);

            if (projections.TryAdd(streamName, true))
            {
                var projectionConfig =
                    $@"fromAll()
                        .when({{
                            $any: function (s, e) {{
                                if (e.streamId.indexOf('{prefix}') === 0 && e.data.{property}) {{
                                    linkTo('{streamName}-' + e.data.{property}, e);
                                }}
                            }}
                        }});";

                try
                {
                    var credentials = connection.Settings.DefaultUserCredentials;

                    await projectionsManager.CreateContinuousAsync($"{streamName}", projectionConfig, credentials);
                }
                catch (Exception ex)
                {
                    if (!ex.Is<ProjectionCommandConflictException>())
                    {
                        throw;
                    }
                }
            }

            return $"{streamName}-{value}";
        }

        public async Task<string> CreateProjectionAsync(string streamFilter = null)
        {
            streamFilter = streamFilter ?? ".*";

            var streamName = CreateFilterStreamName(streamFilter);

            if (projections.TryAdd(streamName, true))
            {
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

                    await projectionsManager.CreateContinuousAsync($"{streamName}", projectionConfig, credentials);
                }
                catch (Exception ex)
                {
                    if (!ex.Is<ProjectionCommandConflictException>())
                    {
                        throw;
                    }
                }
            }

            return streamName;
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
