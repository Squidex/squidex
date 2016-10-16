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
        private readonly IEnumerable<ICommandHandler> handlers;

        public InMemoryCommandBus(IEnumerable<ICommandHandler> handlers)
        {
            Guard.NotNull(handlers, nameof(handlers));
            
            this.handlers = handlers;
        }

        public async Task<CommandContext> PublishAsync(ICommand command)
        {
            Guard.NotNull(command, nameof(command));

            var context = new CommandContext(command);

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

            if (context.Exception != null)
            {
                throw context.Exception;
            }

            return context;
        }
    }
}