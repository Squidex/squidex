// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using NodaTime;

namespace Squidex.Infrastructure.Commands
{
    public sealed class EnrichWithTimestampCommandMiddleware : ICommandMiddleware
    {
        private readonly IClock clock;

        public EnrichWithTimestampCommandMiddleware(IClock clock)
        {
            Guard.NotNull(clock, nameof(clock));

            this.clock = clock;
        }

        public Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is ITimestampCommand timestampCommand)
            {
                timestampCommand.Timestamp = clock.GetCurrentInstant();
            }

            return next();
        }
    }
}
