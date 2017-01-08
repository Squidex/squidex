// ==========================================================================
//  LogExceptionHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

// ReSharper disable InvertIf

namespace Squidex.Infrastructure.CQRS.Commands
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
                logger.LogError(InfrastructureErrors.CommandFailed, exception, "Handling {0} command failed", context.Command);
            }

            return Task.FromResult(false);
        }
    }
}
