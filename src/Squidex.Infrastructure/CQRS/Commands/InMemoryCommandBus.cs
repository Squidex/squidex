// ==========================================================================
//  InMemoryCommandBus.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class InMemoryCommandBus : ICommandBus
    {
        private readonly List<ICommandHandler> handlers;

        public InMemoryCommandBus(IEnumerable<ICommandHandler> handlers)
        {
            Guard.NotNull(handlers, nameof(handlers));

            this.handlers = handlers.Reverse().ToList();
        }

        public async Task<CommandContext> PublishAsync(ICommand command)
        {
            Guard.NotNull(command, nameof(command));

            var context = new CommandContext(command);

            var next = new Func<Task>(() => TaskHelper.Done);

            foreach (var handler in handlers)
            {
                next = Join(handler, context, next);
            }

            await next();

            return context;
        }

        private static Func<Task> Join(ICommandHandler handler, CommandContext context, Func<Task> next)
        {
            return () => handler.HandleAsync(context, next);
        }
    }
}