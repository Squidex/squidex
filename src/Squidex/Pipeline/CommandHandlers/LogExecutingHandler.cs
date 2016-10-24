// ==========================================================================
//  LogExecutingHandler.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PinkParrot.Infrastructure.CQRS.Commands;

namespace PinkParrot.Pipeline.CommandHandlers
{
    public sealed class LogExecutingHandler : ICommandHandler
    {
        private readonly ILogger<LogExecutingHandler> logger;

        public LogExecutingHandler(ILogger<LogExecutingHandler> logger)
        {
            this.logger = logger;
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            logger.LogError("Handling {0} command", context.Command);

            return Task.FromResult(false);
        }
    }
}
