// ==========================================================================
//  LogExecutingHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Commands
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
            logger.LogInformation("Handling {0} command", context.Command);

            return TaskHelper.False;
        }
    }
}
