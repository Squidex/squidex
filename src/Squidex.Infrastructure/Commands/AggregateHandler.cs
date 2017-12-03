// ==========================================================================
//  AggregateHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Commands
{
    public sealed class AggregateHandler : IAggregateHandler
    {
        private readonly IStateFactory stateFactory;
        private readonly IServiceProvider serviceProvider;

        public AggregateHandler(IStateFactory stateFactory, IServiceProvider serviceProvider)
        {
            Guard.NotNull(stateFactory, nameof(stateFactory));
            Guard.NotNull(serviceProvider, nameof(serviceProvider));

            this.stateFactory = stateFactory;
            this.serviceProvider = serviceProvider;
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
            var aggregateFactory = (DomainObjectFactoryFunction<T>)serviceProvider.GetService(typeof(DomainObjectFactoryFunction<T>));

            var wrapper = await stateFactory.GetDetachedAsync<DomainObjectWrapper<T>>(aggregateCommand.AggregateId.ToString());

            var domainObject = aggregateFactory(aggregateCommand.AggregateId);

            await wrapper.LoadAsync(domainObject, isUpdate ? aggregateCommand.ExpectedVersion : -1);
            await wrapper.UpdateAsync(handler);

            if (!context.IsCompleted)
            {
                if (isUpdate)
                {
                    context.Complete(new EntitySavedResult(domainObject.Version));
                }
                else
                {
                    context.Complete(EntityCreatedResult.Create(domainObject.Id, domainObject.Version));
                }
            }

            return domainObject;
        }

        private static IAggregateCommand GetCommand(CommandContext context)
        {
            if (!(context.Command is IAggregateCommand command))
            {
                throw new ArgumentException("Context must have an aggregate command.", nameof(context));
            }

            Guard.NotEmpty(command.AggregateId, "context.Command.AggregateId");

            return command;
        }
    }
}
