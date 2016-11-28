// ==========================================================================
//  EventStoreDomainObjectRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.CQRS.Events;

// ReSharper disable RedundantAssignment
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable TooWideLocalVariableScope

namespace Squidex.Infrastructure.CQRS.EventStore
{
    public sealed class EventStoreDomainObjectRepository : IDomainObjectRepository
    {
        private const int WritePageSize = 500;
        private const int ReadPageSize = 500;
        private readonly IEventStoreConnection connection;
        private readonly IStreamNameResolver nameResolver;
        private readonly IDomainObjectFactory factory;
        private readonly UserCredentials credentials;
        private readonly EventStoreFormatter formatter;

        public EventStoreDomainObjectRepository(
            IDomainObjectFactory factory, 
            IStreamNameResolver nameResolver,
            IEventStoreConnection connection,
            UserCredentials credentials,
            EventStoreFormatter formatter)
        {
            Guard.NotNull(factory, nameof(factory));
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(connection, nameof(connection));
            Guard.NotNull(credentials, nameof(credentials));
            Guard.NotNull(nameResolver, nameof(nameResolver));

            this.factory = factory;
            this.formatter = formatter;
            this.connection = connection;
            this.credentials = credentials;
            this.nameResolver = nameResolver;
        }

        public async Task<TDomainObject> GetByIdAsync<TDomainObject>(Guid id, int version = int.MaxValue) where TDomainObject : class, IAggregate
        {
            Guard.GreaterThan(version, 0, nameof(version));

            var streamName = nameResolver.GetStreamName(typeof(TDomainObject), id);

            var domainObject = (TDomainObject)factory.CreateNew(typeof(TDomainObject), id);
            
            var sliceStart = 0;
            var sliceCount = 0;

            StreamEventsSlice currentSlice;
            do
            {
                sliceCount = sliceStart + ReadPageSize <= version ? ReadPageSize : version - sliceStart + 1;

                currentSlice = await connection.ReadStreamEventsForwardAsync(streamName, sliceStart, sliceCount, false, credentials);

                if (currentSlice.Status == SliceReadStatus.StreamNotFound)
                {
                    throw new DomainObjectNotFoundException(id.ToString(), typeof(TDomainObject));
                }

                if (currentSlice.Status == SliceReadStatus.StreamDeleted)
                {
                    throw new DomainObjectDeletedException(id.ToString(), typeof(TDomainObject));
                }

                sliceStart = currentSlice.NextEventNumber;

                foreach (var resolved in currentSlice.Events)
                {
                    var envelope = formatter.Parse(new EventWrapper(resolved));

                    domainObject.ApplyEvent(envelope);
                }
            }
            while (version >= currentSlice.NextEventNumber && !currentSlice.IsEndOfStream);

            if (domainObject.Version != version && version < int.MaxValue)
            {
                throw new DomainObjectVersionException(id.ToString(), typeof(TDomainObject), domainObject.Version, version);
            }

            return domainObject;
        }

        public async Task SaveAsync(IAggregate domainObject, ICollection<Envelope<IEvent>> events, Guid commitId)
        {
            Guard.NotNull(domainObject, nameof(domainObject));

            var streamName = nameResolver.GetStreamName(domainObject.GetType(), domainObject.Id);

            var versionCurrent = domainObject.Version;
            var versionBefore = versionCurrent - events.Count;
            var versionExpected = versionBefore == 0 ? ExpectedVersion.NoStream : versionBefore - 1;

            var eventsToSave = events.Select(x => formatter.ToEventData(x, commitId)).ToList();

            await InsertEventsAsync(streamName, versionExpected, eventsToSave);

            domainObject.ClearUncommittedEvents();
        }

        private async Task InsertEventsAsync(string streamName, int expectedVersion, IReadOnlyCollection<EventData> eventsToSave)
        {
            if (eventsToSave.Count > 0)
            {
                if (eventsToSave.Count < WritePageSize)
                {
                    await connection.AppendToStreamAsync(streamName, expectedVersion, eventsToSave, credentials);
                }
                else
                {
                    var transaction = await connection.StartTransactionAsync(streamName, expectedVersion, credentials);

                    try
                    {
                        for (var p = 0; p < eventsToSave.Count; p += WritePageSize)
                        {
                            await transaction.WriteAsync(eventsToSave.Skip(p).Take(WritePageSize));
                        }

                        await transaction.CommitAsync();
                    }
                    finally
                    {
                        transaction.Dispose();
                    }
                }
            }
            else
            {
                Debug.WriteLine($"No events to insert for: {streamName}", "GetEventStoreRepository");
            }
        }
    }
}
