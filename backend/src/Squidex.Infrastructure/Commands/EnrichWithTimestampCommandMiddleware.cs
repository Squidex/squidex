// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using NodaTime;

namespace Squidex.Infrastructure.Commands
{
    public sealed class EnrichWithTimestampCommandMiddleware : ICommandMiddleware
    {
        private readonly IClock clock;

        public EnrichWithTimestampCommandMiddleware(IClock clock)
        {
            this.clock = clock;
        }

        public Task HandleAsync(CommandContext context, NextDelegate next)
        {
            if (context.Command is ITimestampCommand timestampCommand)
            {
                timestampCommand.Timestamp = clock.GetCurrentInstant();
            }

            return next(context);
        }
    }
}
