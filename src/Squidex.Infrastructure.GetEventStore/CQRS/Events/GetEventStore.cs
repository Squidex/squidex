// ==========================================================================
//  GetEventStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class GetEventStore : IEventStore, IExternalSystem
    {
        private const int WritePageSize = 500;
        private const int ReadPageSize = 500;
        private readonly IEventStoreConnection connection;
        private readonly string projectionHost;
        private readonly string prefix;

        public GetEventStore(IEventStoreConnection connection, string prefix, string projectionHost)
        {
            Guard.NotNull(connection, nameof(connection));

            this.connection = connection;
            this.projectionHost = projectionHost;

            this.prefix = prefix?.Trim(' ', '-').WithFallback("squidex");
        }

        public void Connect()
        {
            try
            {
                connection.ConnectAsync().Wait();
            }
            catch (Exception ex)
            {
                throw new ConfigurationException("Cannot connect to event store.", ex);
            }
        }

        public IEventSubscription CreateSubscription(IEventSubscriber subscriber, string streamFilter, string position = null)
        {
            Guard.NotNull(subscriber, nameof(subscriber));
            Guard.NotNullOrEmpty(streamFilter, nameof(streamFilter));

            return new GetEventStoreSubscription(connection, subscriber, projectionHost, prefix, position, streamFilter);
        }

        public async Task<IReadOnlyList<StoredEvent>> GetEventsAsync(string streamName)
        {
            var result = new List<StoredEvent>();

            var sliceStart = 0L;

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
            return AppendEventsInternalAsync(streamName, ExpectedVersion.Any, events);
        }

        public Task AppendEventsAsync(Guid commitId, string streamName, int expectedVersion, ICollection<EventData> events)
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
