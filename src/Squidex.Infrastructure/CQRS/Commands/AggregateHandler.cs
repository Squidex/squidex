// ==========================================================================
//  AggregateHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Events;

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class AggregateHandler : IAggregateHandler
    {
        private readonly IDomainObjectRepository domainObjectRepository;
        private readonly IDomainObjectFactory domainObjectFactory;

        public AggregateHandler(
            IDomainObjectFactory domainObjectFactory,
            IDomainObjectRepository domainObjectRepository)
        {
            Guard.NotNull(domainObjectFactory, nameof(domainObjectFactory));
            Guard.NotNull(domainObjectRepository, nameof(domainObjectRepository));

            this.domainObjectFactory = domainObjectFactory;
            this.domainObjectRepository = domainObjectRepository;
        }

        public Task<T> CreateAsync<T>(CommandContext context, Func<T, Task> creator) where T : class, IAggregate
        {
            Guard.NotNull(creator, nameof(creator));

            return InvokeAsync(context, creator, false);
        }

        public Task<T> UpdateAsync<T>(CommandContext context, Func<T, Task> updater) where T : class, IAggregate
        {
            Guard.NotNull(updater, nameof(updater));

            return InvokeAsync(context, updater, true);
        }

        private async Task<T> InvokeAsync<T>(CommandContext context, Func<T, Task> handler, bool isUpdate) where T : class, IAggregate
        {
            Guard.NotNull(context, nameof(context));

            var aggregateCommand = GetCommand(context);
            var aggregateObject = domainObjectFactory.CreateNew<T>(aggregateCommand.AggregateId);

            if (isUpdate)
            {
                await domainObjectRepository.LoadAsync(aggregateObject, aggregateCommand.ExpectedVersion);
            }

            await handler(aggregateObject);

            var events = aggregateObject.GetUncomittedEvents();

            foreach (var @event in events)
            {
                @event.SetAggregateId(aggregateObject.Id);
            }

            await domainObjectRepository.SaveAsync(aggregateObject, events, Guid.NewGuid());

            aggregateObject.ClearUncommittedEvents();

            if (!context.IsCompleted)
            {
                if (isUpdate)
                {
                    context.Complete(new EntitySavedResult(aggregateObject.Version));
                }
                else
                {
                    context.Complete(EntityCreatedResult.Create(aggregateObject.Id, aggregateObject.Version));
                }
            }

            return aggregateObject;
        }

        private static IAggregateCommand GetCommand(CommandContext context)
        {
            var command = context.Command as IAggregateCommand;

            if (command == null)
            {
                throw new ArgumentException("Context must have an aggregate command.", nameof(context));
            }

            Guard.NotEmpty(command.AggregateId, "context.Command.AggregateId");

            return command;
        }
    }
}
