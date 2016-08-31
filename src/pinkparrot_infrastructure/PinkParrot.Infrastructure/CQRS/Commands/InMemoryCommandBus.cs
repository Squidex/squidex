// ==========================================================================
//  InMemoryCommandBus.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PinkParrot.Infrastructure.CQRS.Commands
{
    public sealed class InMemoryCommandBus : ICommandBus
    {
        private readonly IDomainObjectFactory factory;
        private readonly IDomainObjectRepository repository;
        private readonly IEnumerable<ICommandHandler> handlers;

        public InMemoryCommandBus(
            IDomainObjectRepository repository,
            IDomainObjectFactory factory,
            IEnumerable<ICommandHandler> handlers)
        {
            Guard.NotNull(factory, nameof(factory));
            Guard.NotNull(handlers, nameof(handlers));
            Guard.NotNull(repository, nameof(repository));

            this.factory = factory;
            this.handlers = handlers;
            this.repository = repository;
        }

        public async Task PublishAsync(ICommand command)
        {
            Guard.NotNull(command, nameof(command));

            var context = new CommandContext(factory, repository, command);

            foreach (var handler in handlers)
            {
                try
                {
                    var isHandled = await handler.HandleAsync(context);

                    if (isHandled)
                    {
                        context.MarkSucceeded();
                    }
                }
                catch (Exception e)
                {
                    context.MarkFailed(e);
                }
            }
        }
    }
}