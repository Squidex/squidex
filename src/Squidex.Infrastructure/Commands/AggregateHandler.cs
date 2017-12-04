// ==========================================================================
//  AggregateHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Commands
{
    public sealed class AggregateHandler : IAggregateHandler
    {
        private readonly IStateFactory stateFactory;
        private readonly ISemanticLog log;
        private readonly IServiceProvider serviceProvider;

        public AggregateHandler(IStateFactory stateFactory, IServiceProvider serviceProvider, ISemanticLog log)
        {
            Guard.NotNull(stateFactory, nameof(stateFactory));
            Guard.NotNull(serviceProvider, nameof(serviceProvider));
            Guard.NotNull(log, nameof(log));

            this.stateFactory = stateFactory;
            this.serviceProvider = serviceProvider;

            this.log = log;
        }

        public Task<T> CreateAsync<T>(CommandContext context, Func<T, Task> creator) where T : class, IDomainObject
        {
            Guard.NotNull(creator, nameof(creator));

            return InvokeAsync(context, creator, false);
        }

        public Task<T> UpdateAsync<T>(CommandContext context, Func<T, Task> updater) where T : class, IDomainObject
        {
            Guard.NotNull(updater, nameof(updater));

            return InvokeAsync(context, updater, true);
        }

        private async Task<T> InvokeAsync<T>(CommandContext context, Func<T, Task> handler, bool isUpdate) where T : class, IDomainObject
        {
            Guard.NotNull(context, nameof(context));

            var domainObjectCommand = GetCommand(context);
            var domainObjectId = domainObjectCommand.AggregateId;
            var domainObject = await stateFactory.GetDetachedAsync<T>(domainObjectId.ToString());

            await domainObject.WriteAsync(log);

            if (!context.IsCompleted)
            {
                if (isUpdate)
                {
                    context.Complete(new EntitySavedResult(domainObject.Version));
                }
                else
                {
                    context.Complete(EntityCreatedResult.Create(domainObjectId, domainObject.Version));
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
