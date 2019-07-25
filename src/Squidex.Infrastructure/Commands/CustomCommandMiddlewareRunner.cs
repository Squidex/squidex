// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Commands
{
    public sealed class CustomCommandMiddlewareRunner : ICommandMiddleware
    {
        private readonly IEnumerable<ICustomCommandMiddleware> extensions;

        public CustomCommandMiddlewareRunner(IEnumerable<ICustomCommandMiddleware> extensions)
        {
            Guard.NotNull(extensions, nameof(extensions));

            this.extensions = extensions.Reverse().ToList();
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            foreach (var handler in extensions)
            {
                next = Join(handler, context, next);
            }

            await next();
        }

        private static Func<Task> Join(ICommandMiddleware handler, CommandContext context, Func<Task> next)
        {
            return () => handler.HandleAsync(context, next);
        }
    }
}
