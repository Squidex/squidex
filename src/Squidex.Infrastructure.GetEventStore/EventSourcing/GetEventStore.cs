// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class GetEventStore : IEventStore, IInitializable
    {
        private const int WritePageSize = 500;
        private const int ReadPageSize = 500;
        private readonly IEventStoreConnection connection;
        private readonly string prefix;
        private readonly ProjectionClient projectionClient;

        public GetEventStore(IEventStoreConnection connection, string prefix, string projectionHost)
        {
            Guard.NotNull(connection, nameof(connection));

            this.connection = connection;

            this.prefix = prefix?.Trim(' ', '-').WithFallback("squidex");

            projectionClient = new ProjectionClient(connection, prefix, projectionHost);
        }

        public async Task InitializeAsync(CancellationToken ct = default(CancellationToken))
        {
            try
            {
                await connection.ConnectAsync();
            }
            catch (Exception ex)
            {
                throw new ConfigurationException("Cannot connect to event store.", ex);
            }

            await projectionClient.ConnectAsync();
        }

        public IEventSubscription CreateSubscription(IEventSubscriber subscriber, string streamFilter, string position = null)
        {
            return new GetEventStoreSubscription(connection, subscriber, projectionClient, position, streamFilter);
        }

        public Task CreateIndexAsync(string property)
        {
            return projectionClient.CreateProjectionAsync(property, string.Empty);
        }

        public async Task QueryAsync(Func<StoredEvent, Task> callback, string property, object value, string position = null, CancellationToken ct = default(CancellationToken))
        {
            using (Profiler.TraceMethod<GetEventStore>())
            {
                var streamName = await projectionClient.CreateProjectionAsync(property, value);

                var sliceStart = projectionClient.ParsePosition(position);

                await QueryAsync(callback, streamName, sliceStart, ct);
            }
        }

        public async Task QueryAsync(Func<StoredEvent, Task> callback, string streamFilter = null, string position = null, CancellationToken ct = default(CancellationToken))
        {
            using (Profiler.TraceMethod<GetEventStore>())
            {
                var streamName = await projectionClient.CreateProjectionAsync(streamFilter);

                var sliceStart = projectionClient.ParsePosition(position);

                await QueryAsync(callback, streamName, sliceStart, ct);
            }
        }

        private async Task QueryAsync(Func<StoredEvent, Task> callback, string streamName, long sliceStart, CancellationToken ct = default(CancellationToken))
        {
            StreamEventsSlice currentSlice;
            do
            {
                currentSlice = await connection.ReadStreamEventsForwardAsync(streamName, sliceStart, ReadPageSize, false);

                if (currentSlice.Status == SliceReadStatus.Success)
                {
                    sliceStart = currentSlice.NextEventNumber;

                    foreach (var resolved in currentSlice.Events)
                    {
                        var storedEvent = Formatter.Read(resolved);

                        await callback(storedEvent);
                    }
                }
            }
            while (!currentSlice.IsEndOfStream && !ct.IsCancellationRequested);
        }

        public async Task<IReadOnlyList<StoredEvent>> QueryAsync(string streamName, long streamPosition = 0)
        {
            using (Profiler.TraceMethod<GetEventStore>())
            {
                var result = new List<StoredEvent>();

                var sliceStart = streamPosition;

                StreamEventsSlice currentSlice;
                do
                {
                    currentSlice = await connection.ReadStreamEventsForwardAsync(streamName, sliceStart, ReadPageSize, false);

                    if (currentSlice.Status == SliceReadStatus.Success)
                    {
                        sliceStart = currentSlice.NextEventNumber;

                        foreach (var resolved in currentSlice.Events)
                        {
                            var storedEvent = Formatter.Read(resolved);

                            result.Add(storedEvent);
                        }
                    }
                }
                while (!currentSlice.IsEndOfStream);

                return result;
            }
        }

        public Task DeleteStreamAsync(string streamName)
        {
            return connection.DeleteStreamAsync(streamName, ExpectedVersion.Any);
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
            using (Profiler.TraceMethod<GetEventStore>(nameof(AppendAsync)))
            {
                Guard.NotNullOrEmpty(streamName, nameof(streamName));
                Guard.NotNull(events, nameof(events));

                if (events.Count == 0)
                {
                    return;
                }

                var eventsToSave = events.Select(Formatter.Write).ToList();

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
        }

        public Task DeleteManyAsync(string property, object value)
        {
            throw new NotSupportedException();
        }

        private string GetStreamName(string streamName)
        {
            return $"{prefix}-{streamName}";
        }
    }
}
