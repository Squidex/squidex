// ==========================================================================
//  DefaultDomainObjectRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class DefaultDomainObjectRepository : IDomainObjectRepository
    {
        private readonly IStreamNameResolver nameResolver;
        private readonly IDomainObjectFactory factory;
        private readonly IEventStore eventStore;
        private readonly EventDataFormatter formatter;

        public DefaultDomainObjectRepository(
            IDomainObjectFactory factory,
            IEventStore eventStore,
            IStreamNameResolver nameResolver,
            EventDataFormatter formatter)
        {
            Guard.NotNull(factory, nameof(factory));
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(nameResolver, nameof(nameResolver));

            this.factory = factory;
            this.formatter = formatter;
            this.eventStore = eventStore;
            this.nameResolver = nameResolver;
        }

        public async Task<TDomainObject> GetByIdAsync<TDomainObject>(Guid id, long? expectedVersion = null) where TDomainObject : class, IAggregate
        {
            var streamName = nameResolver.GetStreamName(typeof(TDomainObject), id);

            var events = await eventStore.GetEventsAsync(streamName);

            if (events.Count == 0)
            {
                throw new DomainObjectNotFoundException(id.ToString(), typeof(TDomainObject));
            }

            var domainObject = (TDomainObject)factory.CreateNew(typeof(TDomainObject), id);

            foreach (var storedEvent in events)
            {
                var envelope = ParseOrNull(storedEvent);

                if (envelope != null)
                {
                    domainObject.ApplyEvent(envelope);
                }
            }

            if (expectedVersion != null && domainObject.Version != expectedVersion.Value)
            {
                throw new DomainObjectVersionException(id.ToString(), typeof(TDomainObject), domainObject.Version, expectedVersion.Value);
            }

            return domainObject;
        }

        public async Task SaveAsync(IAggregate domainObject, ICollection<Envelope<IEvent>> events, Guid commitId)
        {
            Guard.NotNull(domainObject, nameof(domainObject));

            var streamName = nameResolver.GetStreamName(domainObject.GetType(), domainObject.Id);

            var versionCurrent = domainObject.Version;
            var versionExpected = versionCurrent - events.Count;

            var eventsToSave = events.Select(x => formatter.ToEventData(x, commitId)).ToList();

            try
            {
                await eventStore.AppendEventsAsync(commitId, streamName, versionExpected, eventsToSave);
            }
            catch (WrongEventVersionException)
            {
                throw new DomainObjectVersionException(domainObject.Id.ToString(), domainObject.GetType(), versionCurrent, versionExpected);
            }
        }

        private Envelope<IEvent> ParseOrNull(StoredEvent storedEvent)
        {
            try
            {
                return formatter.Parse(storedEvent.Data);
            }
            catch (TypeNameNotFoundException)
            {
                return null;
            }
        }
    }
}
