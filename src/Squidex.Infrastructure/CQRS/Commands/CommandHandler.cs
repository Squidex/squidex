// ==========================================================================
//  CommandHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public abstract class CommandHandler<T> : ICommandHandler where T : class, IAggregate
    {
        private readonly IDomainObjectRepository domainObjectRepository;
        private readonly IDomainObjectFactory domainObjectFactory;

        protected IDomainObjectRepository Repository
        {
            get { return domainObjectRepository; }
        }

        protected IDomainObjectFactory Factory
        {
            get { return domainObjectFactory; }
        }

        protected CommandHandler(IDomainObjectFactory domainObjectFactory, IDomainObjectRepository domainObjectRepository)
        {
            Guard.NotNull(domainObjectFactory, nameof(domainObjectFactory));
            Guard.NotNull(domainObjectRepository, nameof(domainObjectRepository));

            this.domainObjectFactory = domainObjectFactory;
            this.domainObjectRepository = domainObjectRepository;
        }

        protected async Task CreateAsync(IAggregateCommand command, Func<T, Task> creator)
        {
            Guard.NotNull(creator, nameof(creator));
            Guard.NotNull(command, nameof(command));

            var domainObject = domainObjectFactory.CreateNew<T>(command.AggregateId);

            await creator(domainObject);

            await domainObjectRepository.SaveAsync(domainObject, Guid.NewGuid());
        }

        protected Task CreateAsync(IAggregateCommand command, Action<T> creator)
        {
            return CreateAsync(command, x =>
            {
                creator(x);
                return Task.FromResult(true);
            });
        }

        protected async Task UpdateAsync(IAggregateCommand command, Func<T, Task> updater)
        {
            Guard.NotNull(updater, nameof(updater));
            Guard.NotNull(command, nameof(command));

            var domainObject = await domainObjectRepository.GetByIdAsync<T>(command.AggregateId);

            await updater(domainObject);

            await domainObjectRepository.SaveAsync(domainObject, Guid.NewGuid());
        }

        protected Task UpdateAsync(IAggregateCommand command, Action<T> updater)
        {
            return UpdateAsync(command, x =>
            {
                updater(x);
                return Task.FromResult(true);
            });
        }

        public abstract Task<bool> HandleAsync(CommandContext context);
    }
}
