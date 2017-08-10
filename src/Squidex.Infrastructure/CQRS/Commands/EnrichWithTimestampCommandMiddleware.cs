// ==========================================================================
//  EnrichWithTimestampCommandMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using NodaTime;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class EnrichWithTimestampHandler : ICommandMiddleware
    {
        private readonly IClock clock;

        public EnrichWithTimestampHandler(IClock clock)
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
