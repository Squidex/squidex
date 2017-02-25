// ==========================================================================
//  EnrichWithTimestampHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using NodaTime;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class EnrichWithTimestampHandler : ICommandHandler
    {
        private readonly IClock clock;

        public EnrichWithTimestampHandler(IClock clock)
        {
            Guard.NotNull(clock, nameof(clock));

            this.clock = clock;
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            var timestampCommand = context.Command as ITimestampCommand;

            if (timestampCommand != null)
            {
                timestampCommand.Timestamp = clock.GetCurrentInstant();
            }

            return Task.FromResult(false);
        }
    }
}
