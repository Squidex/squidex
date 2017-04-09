// ==========================================================================
//  LogExecutingHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class LogExecutingHandler : ICommandHandler
    {
        private readonly ISemanticLog log;

        public LogExecutingHandler(ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));

            this.log = log;
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            log.LogInformation(w => w
                .WriteProperty("action", "HandleCommand.")
                .WriteProperty("actionId", context.ContextId.ToString())
                .WriteProperty("state", "Started")
                .WriteProperty("commandType", context.Command.GetType().Name));

            return TaskHelper.False;
        }
    }
}
