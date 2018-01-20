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
using EventStore.ClientAPI.Projections;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class GetEventStore : IEventStore, IInitializable
    {
        private const int WritePageSize = 500;
        private const int ReadPageSize = 500;
        private readonly IEventStoreConnection connection;
        private readonly string projectionHost;
        private readonly string prefix;
        private ProjectionsManager projectionsManager;

        public GetEventStore(IEventStoreConnection connection, string prefix, string projectionHost)
        {
            Guard.NotNull(connection, nameof(connection));

            this.connection = connection;
            this.projectionHost = projectionHost;

            this.prefix = prefix?.Trim(' ', '-').WithFallback("squidex");
        }

        public void Initialize()
        {
            try
            {
                connection.ConnectAsync().Wait();
            }
            catch (Exception ex)
            {
                throw new ConfigurationException("Cannot connect to event store.", ex);
            }

            try
            {
                projectionsManager = connection.GetProjectionsManagerAsync(projectionHost).Result;

                projectionsManager.ListAllAsync(connection.Settings.DefaultUserCredentials).Wait();
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Cannot connect to event store projections: {projectionHost}.", ex);
            }
        }

        public IEventSubscription CreateSubscription(IEventSubscriber subscriber, string streamFilter, string position = null)
        {
            return new GetEventStoreSubscription(connection, subscriber, projectionsManager, prefix, position, streamFilter);
        }

        public async Task GetEventsAsync(Func<StoredEvent, Task> callback, string streamFilter = null, string position = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var streamName = await connection.CreateProjectionAsync(projectionsManager, prefix, streamFilter);

            var sliceStart = ProjectionHelper.ParsePosition(position);

            StreamEventsSlice currentSlice;
            do
            {
                currentSlice = await connection.ReadStreamEventsForwardAsync(streamName, sliceStart, ReadPageSize, true);

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
            while (!currentSlice.IsEndOfStream && !cancellationToken.IsCancellationRequested);
        }

        public async Task<IReadOnlyList<StoredEvent>> GetEventsAsync(string streamName, long streamPosition = 0)
        {
            var result = new List<StoredEvent>();

            var sliceStart = streamPosition;

            StreamEventsSlice currentSlice;
            do
            {
                currentSlice = await connection.ReadStreamEventsForwardAsync(GetStreamName(streamName), sliceStart, ReadPageSize, false);

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

        public Task AppendEventsAsync(Guid commitId, string streamName, ICollection<EventData> events)
        {
            return AppendEventsInternalAsync(streamName, EtagVersion.Any, events);
        }

        public Task AppendEventsAsync(Guid commitId, string streamName, long expectedVersion, ICollection<EventData> events)
        {
            Guard.GreaterEquals(expectedVersion, -1, nameof(expectedVersion));

            return AppendEventsInternalAsync(streamName, expectedVersion, events);
        }

        private async Task AppendEventsInternalAsync(string streamName, long expectedVersion, ICollection<EventData> events)
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

        private string GetStreamName(string streamName)
        {
            return $"{prefix}-{streamName}";
        }
    }
}
