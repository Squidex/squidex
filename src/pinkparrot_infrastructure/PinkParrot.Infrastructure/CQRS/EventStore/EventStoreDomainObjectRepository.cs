// ==========================================================================
//  GetEventStoreDomainObjectRepository.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using PinkParrot.Infrastructure.CQRS.Commands;
// ReSharper disable RedundantAssignment

// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable TooWideLocalVariableScope

namespace PinkParrot.Infrastructure.CQRS.EventStore
{
    public sealed class EventStoreDomainObjectRepository : IDomainObjectRepository
    {
        private const int WritePageSize = 500;
        private const int ReadPageSize = 500;
        private readonly IEventStoreConnection connection;
        private readonly IStreamNameResolver nameResolver;
        private readonly IDomainObjectFactory factory;
        private readonly UserCredentials credentials;
        private readonly EventStoreParser formatter;

        public EventStoreDomainObjectRepository(
            IDomainObjectFactory factory, 
            IStreamNameResolver nameResolver,
            IEventStoreConnection connection,
            UserCredentials credentials,
            EventStoreParser formatter)
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

        public async Task<TDomainObject> GetByIdAsync<TDomainObject>(Guid id, int version = 0) where TDomainObject : class, IAggregate
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
                    var envelope = formatter.Parse(resolved);

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

        public async Task SaveAsync(IAggregate domainObject, Guid commitId)
        {
            Guard.NotNull(domainObject, nameof(domainObject));

            var streamName = nameResolver.GetStreamName(domainObject.GetType(), domainObject.Id);

            var newEvents = domainObject.GetUncomittedEvents();

            var currVersion = domainObject.Version;
            var prevVersion = currVersion - newEvents.Count;
            var exptVersion = prevVersion == 0 ? ExpectedVersion.NoStream : prevVersion - 1;

            var eventsToSave = newEvents.Select(x => formatter.ToEventData(x, commitId)).ToList();

            await InsertEventsAsync(streamName, exptVersion, eventsToSave);

            domainObject.ClearUncommittedEvents();
        }

        private async Task InsertEventsAsync(string streamName, int exptVersion, IReadOnlyCollection<EventData> eventsToSave)
        {
            if (eventsToSave.Count > 0)
            {
                if (eventsToSave.Count < WritePageSize)
                {
                    await connection.AppendToStreamAsync(streamName, exptVersion, eventsToSave, credentials);
                }
                else
                {
                    var transaction = await connection.StartTransactionAsync(streamName, exptVersion, credentials);

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
                Debug.WriteLine(string.Format("No events to insert for: {0}", streamName), "GetEventStoreRepository");
            }
        }
    }
}
