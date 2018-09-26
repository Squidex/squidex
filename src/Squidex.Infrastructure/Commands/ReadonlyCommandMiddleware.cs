// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Squidex.Infrastructure.Commands
{
    public sealed class ReadonlyCommandMiddleware : ICommandMiddleware
    {
        private readonly IOptions<ReadonlyOptions> options;

        public ReadonlyCommandMiddleware(IOptions<ReadonlyOptions> options)
        {
            Guard.NotNull(options, nameof(options));

            this.options = options;
        }

        public Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (options.Value.IsReadonly)
            {
                throw new DomainException("Application is in readonly mode at the moment.");
            }

            return next();
        }
    }
}
