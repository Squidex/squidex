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
using System.Reactive.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class DefaultDomainObjectRepository : IDomainObjectRepository
    {
        private readonly IStreamNameResolver nameResolver;
        private readonly IDomainObjectFactory factory;
        private readonly IEventStore eventStore;
        private readonly IEventPublisher eventPublisher;
        private readonly EventDataFormatter formatter;

        public DefaultDomainObjectRepository(
            IDomainObjectFactory factory, 
            IEventStore eventStore,
            IEventPublisher eventPublisher,
            IStreamNameResolver nameResolver,
            EventDataFormatter formatter)
        {
            Guard.NotNull(factory, nameof(factory));
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventPublisher, nameof(eventPublisher));
            Guard.NotNull(nameResolver, nameof(nameResolver));

            this.factory = factory;
            this.eventStore = eventStore;
            this.formatter = formatter;
            this.eventPublisher = eventPublisher;
            this.nameResolver = nameResolver;
        }

        public async Task<TDomainObject> GetByIdAsync<TDomainObject>(Guid id, int version = int.MaxValue) where TDomainObject : class, IAggregate
        {
            Guard.GreaterThan(version, 0, nameof(version));

            var streamName = nameResolver.GetStreamName(typeof(TDomainObject), id);

            var domainObject = (TDomainObject)factory.CreateNew(typeof(TDomainObject), id);

            var events = await eventStore.GetEventsAsync(streamName).ToList();

            if (events.Count == 0)
            {
                throw new DomainObjectNotFoundException(id.ToString(), typeof(TDomainObject));
            }

            foreach (var eventData in events)
            {
                var envelope = formatter.Parse(eventData);

                domainObject.ApplyEvent(envelope);
            }

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
            var versionExpected = versionBefore == 0 ? -1 : versionBefore - 1;

            var eventsToSave = events.Select(x => formatter.ToEventData(x, commitId)).ToList();

            await eventStore.AppendEventsAsync(commitId, streamName, versionExpected, eventsToSave);

            foreach (var eventData in eventsToSave)
            {
                eventPublisher.Publish(eventData);
            }
        }
    }
}
