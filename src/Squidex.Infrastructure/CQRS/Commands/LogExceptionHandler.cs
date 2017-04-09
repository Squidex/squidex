// ==========================================================================
//  LogExceptionHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;

// ReSharper disable InvertIf

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class LogExceptionHandler : ICommandHandler
    {
        private readonly ISemanticLog log;

        public LogExceptionHandler(ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));

            this.log = log;
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            var exception = context.Exception;

            if (exception != null)
            {
                log.LogError(exception, w => w
                    .WriteProperty("action", "HandleCommand.")
                    .WriteProperty("actionId", context.ContextId.ToString())
                    .WriteProperty("state", "Failed")
                    .WriteProperty("commandType", context.Command.GetType().Name));
            }

            if (!context.IsHandled)
            {
                log.LogFatal(exception, w => w
                    .WriteProperty("action", "HandleCommand.")
                    .WriteProperty("actionId", context.ContextId.ToString())
                    .WriteProperty("state", "Unhandled")
                    .WriteProperty("commandType", context.Command.GetType().Name));
            }

            return TaskHelper.False;
        }
    }
}
