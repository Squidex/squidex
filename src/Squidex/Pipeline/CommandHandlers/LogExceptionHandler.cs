// ==========================================================================
//  LogExceptionHandler.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PinkParrot.Infrastructure.CQRS.Commands;

// ReSharper disable InvertIf

namespace PinkParrot.Pipeline.CommandHandlers
{
    public sealed class LogExceptionHandler : ICommandHandler
    {
        private readonly ILogger<LogExceptionHandler> logger;

        public LogExceptionHandler(ILogger<LogExceptionHandler> logger)
        {
            this.logger = logger;
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            var exception = context.Exception;

            if (exception != null)
            {
                var eventId = new EventId(9999, "CommandFailed");

                logger.LogError(eventId, exception, "Handling {0} command failed", context.Command);
            }

            return Task.FromResult(false);
        }
    }
}
