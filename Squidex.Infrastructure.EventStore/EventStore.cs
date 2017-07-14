// ==========================================================================
//  EventStore.cs
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
using Squidex.Infrastructure.CQRS.Events;
using EventData = Squidex.Infrastructure.CQRS.Events.EventData;

// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable InvertIf

namespace Squidex.Infrastructure.EventStore
{
    public sealed class EventStore : IEventStore, IExternalSystem
    {
        private const int WritePageSize = 500;
        private const int ReadPageSize = 500;
        private readonly IEventStoreConnection connection;
        private readonly string projectionHost;
        private readonly string prefix;

        public EventStore(IEventStoreConnection connection, string prefix, string projectionHost)
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

        public IEventSubscription CreateSubscription(string streamFilter = null, string position = null)
        {
            return new EventStoreSubscription(connection, streamFilter, position, prefix, projectionHost);
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
                        var eventData = Formatter.Read(resolved.Event);

                        result.Add(new StoredEvent(resolved.OriginalPosition.ToString(), resolved.Event.EventNumber, eventData));
                    }
                }
            }
            while (!currentSlice.IsEndOfStream);

            return result;
        }

        public async Task AppendEventsAsync(Guid commitId, string streamName, int expectedVersion, ICollection<EventData> events)
        {
            Guard.NotNull(events, nameof(events));
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

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
