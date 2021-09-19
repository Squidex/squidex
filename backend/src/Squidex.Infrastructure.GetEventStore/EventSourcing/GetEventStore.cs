// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using Squidex.Hosting;
using Squidex.Hosting.Configuration;
using Squidex.Infrastructure.Json;
using Squidex.Log;

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
            CancellationToken ct = default)
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

        public async IAsyncEnumerable<StoredEvent> QueryAllAsync(string? streamFilter = null, string? position = null, long take = long.MaxValue,
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

        public async IAsyncEnumerable<StoredEvent> QueryAllReverseAsync(string? streamFilter = null, string? position = null, long take = long.MaxValue,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            if (take <= 0)
            {
                yield break;
            }

            var streamName = await projectionClient.CreateProjectionAsync(streamFilter);

            var sliceStart = ProjectionClient.ParsePosition(position);

            await foreach (var storedEvent in QueryAsync(streamName, sliceStart, take, ct))
            {
                yield return storedEvent;
            }
        }

        public async Task<IReadOnlyList<StoredEvent>> QueryLatestAsync(string streamName, int count)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

            if (count <= 0)
            {
                return EmptyEvents;
            }

            using (Profiler.TraceMethod<GetEventStore>())
            {
                var result = new List<StoredEvent>();

                await foreach (var storedEvent in QueryReverseAsync(streamName, StreamPosition.End, default))
                {
                    result.Add(storedEvent);
                }

                return result.ToList();
            }
        }

        public async Task<IReadOnlyList<StoredEvent>> QueryAsync(string streamName, long streamPosition = 0)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

            using (Profiler.TraceMethod<GetEventStore>())
            {
                var result = new List<StoredEvent>();

                await foreach (var storedEvent in QueryAsync(streamName, StreamPosition.End, default))
                {
                    result.Add(storedEvent);
                }

                return result.ToList();
            }
        }

        private async IAsyncEnumerable<StoredEvent> QueryAsync(string streamName, long sliceStart, long take = int.MaxValue,
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

        public Task DeleteStreamAsync(string streamName)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

            return connection.DeleteStreamAsync(GetStreamName(streamName), ExpectedVersion.Any);
        }

        public Task AppendAsync(Guid commitId, string streamName, ICollection<EventData> events)
        {
            return AppendEventsInternalAsync(streamName, EtagVersion.Any, events);
        }

        public Task AppendAsync(Guid commitId, string streamName, long expectedVersion, ICollection<EventData> events)
        {
            Guard.GreaterEquals(expectedVersion, -1, nameof(expectedVersion));

            return AppendEventsInternalAsync(streamName, expectedVersion, events);
        }

        private async Task AppendEventsInternalAsync(string streamName, long expectedVersion, ICollection<EventData> events)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));
            Guard.NotNull(events, nameof(events));

            using (Profiler.TraceMethod<GetEventStore>(nameof(AppendAsync)))
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
                            for (var p = 0; p < eventsToSave.Count; p += WritePageSize)
                            {
                                await transaction.WriteAsync(eventsToSave.Skip(p).Take(WritePageSize));
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

        private static int ParseVersion(string message)
        {
            return int.Parse(message[(message.LastIndexOf(':') + 1)..]);
        }

        private string GetStreamName(string streamName)
        {
            return $"{prefix}-{streamName}";
        }
    }
}
