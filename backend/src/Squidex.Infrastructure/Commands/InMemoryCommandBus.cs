// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Commands
{
    public sealed class InMemoryCommandBus : ICommandBus
    {
        private readonly IStep pipeline;

        private interface IStep
        {
            Task InvokeAsync(CommandContext context);
        }

        private sealed class NoopStep : IStep
        {
            public Task InvokeAsync(CommandContext context)
            {
                return Task.CompletedTask;
            }
        }

        private sealed class DefaultStep : IStep
        {
            private readonly IStep next;
            private readonly ICommandMiddleware middleware;

            public DefaultStep(ICommandMiddleware middleware, IStep next)
            {
                this.middleware = middleware;

                this.next = next;
            }

            public Task InvokeAsync(CommandContext context)
            {
                return middleware.HandleAsync(context, next.InvokeAsync);
            }
        }

        public InMemoryCommandBus(IEnumerable<ICommandMiddleware> middlewares)
        {
            var reverseMiddlewares = middlewares.Reverse().ToList();

            IStep next = new NoopStep();

            foreach (var middleware in reverseMiddlewares)
            {
                next = new DefaultStep(middleware, next);
            }

            pipeline = next;
        }

        public async Task<CommandContext> PublishAsync(ICommand command)
        {
            Guard.NotNull(command, nameof(command));

            var context = new CommandContext(command, this);

            await pipeline.InvokeAsync(context);

            return context;
        }
    }
}
