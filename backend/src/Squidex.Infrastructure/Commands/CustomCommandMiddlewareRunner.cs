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
    public sealed class CustomCommandMiddlewareRunner : ICommandMiddleware
    {
        private readonly IEnumerable<ICustomCommandMiddleware> extensions;

        public CustomCommandMiddlewareRunner(IEnumerable<ICustomCommandMiddleware> extensions)
        {
            this.extensions = extensions.Reverse().ToList();
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            foreach (var handler in extensions)
            {
                next = Join(handler, next);
            }

            await next(context);
        }

        private static NextDelegate Join(ICommandMiddleware handler, NextDelegate next)
        {
            return context => handler.HandleAsync(context, next);
        }
    }
}
