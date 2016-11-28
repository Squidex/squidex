// ==========================================================================
//  AggregateHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class AggregateHandler : IAggregateHandler
    {
        private readonly IDomainObjectRepository domainObjectRepository;
        private readonly IDomainObjectFactory domainObjectFactory;
        private readonly IEnumerable<IEventProcessor> eventProcessors;

        public IDomainObjectRepository Repository
        {
            get { return domainObjectRepository; }
        }

        public IDomainObjectFactory Factory
        {
            get { return domainObjectFactory; }
        }

        public AggregateHandler(
            IDomainObjectFactory domainObjectFactory, 
            IDomainObjectRepository domainObjectRepository, 
            IEnumerable<IEventProcessor> eventProcessors)
        {
            Guard.NotNull(eventProcessors, nameof(eventProcessors));
            Guard.NotNull(domainObjectFactory, nameof(domainObjectFactory));
            Guard.NotNull(domainObjectRepository, nameof(domainObjectRepository));

            this.domainObjectFactory = domainObjectFactory;
            this.domainObjectRepository = domainObjectRepository;

            this.eventProcessors = eventProcessors;
        }

        public async Task CreateAsync<T>(IAggregateCommand command, Func<T, Task> creator) where T : class, IAggregate
        {
            Guard.NotNull(creator, nameof(creator));
            Guard.NotNull(command, nameof(command));

            var aggregate = domainObjectFactory.CreateNew<T>(command.AggregateId);

            await creator(aggregate);

            await Save(command, aggregate);
        }

        public async Task UpdateAsync<T>(IAggregateCommand command, Func<T, Task> updater) where T : class, IAggregate
        {
            Guard.NotNull(updater, nameof(updater));
            Guard.NotNull(command, nameof(command));

            var aggregate = await domainObjectRepository.GetByIdAsync<T>(command.AggregateId);

            await updater(aggregate);

            await Save(command, aggregate);
        }

        private async Task Save(ICommand command, IAggregate aggregate)
        {
            var events = aggregate.GetUncomittedEvents();

            foreach (var @event in events)
            {
                foreach (var eventProcessor in eventProcessors)
                {
                    await eventProcessor.ProcessEventAsync(@event, aggregate, command);
                }
            }

            await domainObjectRepository.SaveAsync(aggregate, events, Guid.NewGuid());

            aggregate.ClearUncommittedEvents();
        }
    }
}
