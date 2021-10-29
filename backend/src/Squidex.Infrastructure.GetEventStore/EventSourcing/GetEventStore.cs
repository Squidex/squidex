// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using NodaTime;
using Squidex.Hosting;
using Squidex.Hosting.Configuration;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class GetEventStore : IEventStore, IInitializable
    {
        private const int WritePageSize = 500;
        private const int ReadPageSize = 500;
        private static readonly IReadOnlyList<StoredEvent> EmptyEvents = new List<StoredEvent>();
        private readonly IEventStoreConnection connection;
        private readonly IJsonSerializer serializer;
        private readonly string prefix = "squidex";
        private readonly ProjectionClient projectionClient;

        public GetEventStore(IEventStoreConnection connection, IJsonSerializer serializer, string prefix, string projectionHost)
        {
            this.connection = connection;
            this.serializer = serializer;

            if (!string.IsNullOrWhiteSpace(prefix))
            {
                this.prefix = prefix.Trim(' ', '-');
            }

            projectionClient = new ProjectionClient(connection, this.prefix, projectionHost);
        }

        public async Task InitializeAsync(
            CancellationToken ct)
        {
            try
            {
                await connection.ConnectAsync();
            }
            catch (Exception ex)
            {
                var error = new ConfigurationError("GetEventStore cannot connect to event store.");

                throw new ConfigurationException(error, ex);
            }

            await projectionClient.ConnectAsync();
        }

        public IEventSubscription CreateSubscription(IEventSubscriber subscriber, string? streamFilter = null, string? position = null)
        {
            Guard.NotNull(streamFilter, nameof(streamFilter));

            return new GetEventStoreSubscription(connection, subscriber, serializer, projectionClient, position, prefix, streamFilter);
        }

        public async IAsyncEnumerable<StoredEvent> QueryAllAsync(string? streamFilter = null, string? position = null, int take = int.MaxValue,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            if (take <= 0)
            {
                yield break;
            }

            var streamName = await projectionClient.CreateProjectionAsync(streamFilter);

            var sliceStart = ProjectionClient.ParsePosition(position);

            await foreach (var storedEvent in QueryReverseAsync(streamName, sliceStart, take, ct))
            {
                yield return storedEvent;
            }
        }

        public async IAsyncEnumerable<StoredEvent> QueryAllReverseAsync(string? streamFilter = null, Instant timestamp = default, int take = int.MaxValue,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            if (take <= 0)
            {
                yield break;
            }

            var streamName = await projectionClient.CreateProjectionAsync(streamFilter);

            await foreach (var storedEvent in QueryAsync(streamName, StreamPosition.Start, take, ct))
            {
                yield return storedEvent;
            }
        }

        public async Task<IReadOnlyList<StoredEvent>> QueryLatestAsync(string streamName, int count = int.MaxValue,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

            if (count <= 0)
            {
                return EmptyEvents;
            }

            using (Telemetry.Activities.StartActivity("GetEventStore/GetEventStore"))
            {
                var result = new List<StoredEvent>();

                await foreach (var storedEvent in QueryReverseAsync(streamName, StreamPosition.End, default, ct))
                {
                    result.Add(storedEvent);
                }

                return result.ToList();
            }
        }

        public async Task<IReadOnlyList<StoredEvent>> QueryAsync(string streamName, long streamPosition = 0,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

            using (Telemetry.Activities.StartActivity("GetEventStore/QueryAsync"))
            {
                var result = new List<StoredEvent>();

                await foreach (var storedEvent in QueryAsync(streamName, StreamPosition.Start, int.MaxValue, ct))
                {
                    result.Add(storedEvent);
                }

                return result.ToList();
            }
        }

        private async IAsyncEnumerable<StoredEvent> QueryAsync(string streamName, long sliceStart, int take = int.MaxValue,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var taken = take;

            StreamEventsSlice currentSlice;
            do
            {
                currentSlice = await connection.ReadStreamEventsForwardAsync(streamName, sliceStart, ReadPageSize, true);

                if (currentSlice.Status == SliceReadStatus.Success)
                {
                    sliceStart = currentSlice.NextEventNumber;

                    foreach (var resolved in currentSlice.Events)
                    {
                        var storedEvent = Formatter.Read(resolved, prefix, serializer);

                        yield return storedEvent;

                        if (taken == take)
                        {
                            break;
                        }

                        taken++;
                    }
                }
            }
            while (!currentSlice.IsEndOfStream && !ct.IsCancellationRequested && taken < take);
        }

        private async IAsyncEnumerable<StoredEvent> QueryReverseAsync(string streamName, long sliceStart, long take = int.MaxValue,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var taken = take;

            StreamEventsSlice currentSlice;
            do
            {
                currentSlice = await connection.ReadStreamEventsBackwardAsync(streamName, sliceStart, ReadPageSize, true);

                if (currentSlice.Status == SliceReadStatus.Success)
                {
                    sliceStart = currentSlice.NextEventNumber;

                    foreach (var resolved in currentSlice.Events.OrderByDescending(x => x.Event.EventNumber))
                    {
                        var storedEvent = Formatter.Read(resolved, prefix, serializer);

                        yield return storedEvent;

                        if (taken == take)
                        {
                            break;
                        }

                        taken++;
                    }
                }
            }
            while (!currentSlice.IsEndOfStream && !ct.IsCancellationRequested && taken < take);
        }

        public Task DeleteStreamAsync(string streamName,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

            return connection.DeleteStreamAsync(GetStreamName(streamName), ExpectedVersion.Any);
        }

        public Task AppendAsync(Guid commitId, string streamName, ICollection<EventData> events,
            CancellationToken ct = default)
        {
            return AppendEventsInternalAsync(streamName, EtagVersion.Any, events, ct);
        }

        public Task AppendAsync(Guid commitId, string streamName, long expectedVersion, ICollection<EventData> events,
            CancellationToken ct = default)
        {
            Guard.GreaterEquals(expectedVersion, -1, nameof(expectedVersion));

            return AppendEventsInternalAsync(streamName, expectedVersion, events, ct);
        }

        private async Task AppendEventsInternalAsync(string streamName, long expectedVersion, ICollection<EventData> events,
            CancellationToken ct)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));
            Guard.NotNull(events, nameof(events));

            using (Telemetry.Activities.StartActivity("GetEventStore/AppendEventsInternalAsync"))
            {
                if (events.Count == 0)
                {
                    return;
                }

                try
                {
                    var eventsToSave = events.Select(x => Formatter.Write(x, serializer)).ToList();

                    if (eventsToSave.Count < WritePageSize)
                    {
                        await connection.AppendToStreamAsync(GetStreamName(streamName), expectedVersion, eventsToSave);
                    }
                    else
                    {
                        using (var transaction = await connection.StartTransactionAsync(GetStreamName(streamName), expectedVersion))
                        {
                            ct.ThrowIfCancellationRequested();

                            for (var offset = 0; offset < eventsToSave.Count; offset += WritePageSize)
                            {
                                await transaction.WriteAsync(eventsToSave.Skip(offset).Take(WritePageSize));
                            }

                            await transaction.CommitAsync();
                        }
                    }
                }
                catch (WrongExpectedVersionException ex)
                {
                    throw new WrongEventVersionException(ParseVersion(ex.Message), expectedVersion);
                }
            }
        }

        public async Task DeleteAsync(string streamFilter,
            CancellationToken ct = default)
        {
            var streamName = await projectionClient.CreateProjectionAsync(streamFilter);

            var deleted = new HashSet<string>();

            await foreach (var storedEvent in QueryAsync(streamName, StreamPosition.Start, int.MaxValue, ct))
            {
                if (deleted.Add(storedEvent.StreamName))
                {
                    await connection.DeleteStreamAsync(storedEvent.StreamName, ExpectedVersion.Any);
                }
            }
        }

        private static int ParseVersion(string message)
        {
            return int.Parse(message[(message.LastIndexOf(':') + 1)..], CultureInfo.InvariantCulture);
        }

        private string GetStreamName(string streamName)
        {
            return $"{prefix}-{streamName}";
        }
    }
}
