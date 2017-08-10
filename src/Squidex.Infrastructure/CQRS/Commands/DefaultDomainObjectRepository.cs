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
        private readonly IEventStore eventStore;
        private readonly EventDataFormatter formatter;

        public DefaultDomainObjectRepository(IEventStore eventStore, IStreamNameResolver nameResolver, EventDataFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(nameResolver, nameof(nameResolver));

            this.formatter = formatter;
            this.eventStore = eventStore;
            this.nameResolver = nameResolver;
        }

        public async Task LoadAsync(IAggregate domainObject, long? expectedVersion = null)
        {
            var streamName = nameResolver.GetStreamName(domainObject.GetType(), domainObject.Id);

            var events = await eventStore.GetEventsAsync(streamName);

            if (events.Count == 0)
            {
                throw new DomainObjectNotFoundException(domainObject.Id.ToString(), domainObject.GetType());
            }

            foreach (var storedEvent in events)
            {
                var envelope = ParseKnownCommand(storedEvent);

                if (envelope != null)
                {
                    domainObject.ApplyEvent(envelope);
                }
            }

            if (expectedVersion != null && domainObject.Version != expectedVersion.Value)
            {
                throw new DomainObjectVersionException(domainObject.Id.ToString(), domainObject.GetType(), domainObject.Version, expectedVersion.Value);
            }
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

        private Envelope<IEvent> ParseKnownCommand(StoredEvent storedEvent)
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
