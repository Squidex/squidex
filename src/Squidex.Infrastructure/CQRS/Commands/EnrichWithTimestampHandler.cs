// ==========================================================================
//  EnrichWithTimestampHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class EnrichWithTimestampHandler : ICommandHandler
    {
        private readonly Func<DateTime> timestamp;

        public EnrichWithTimestampHandler()
            : this(() => DateTime.UtcNow)
        {
        }

        public EnrichWithTimestampHandler(Func<DateTime> timestamp)
        {
            Guard.NotNull(timestamp, nameof(timestamp));

            this.timestamp = timestamp;
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            var timestampCommand = context.Command as ITimestampCommand;

            if (timestampCommand != null)
            {
                timestampCommand.Timestamp = timestamp();
            }

            return Task.FromResult(false);
        }
    }
}
