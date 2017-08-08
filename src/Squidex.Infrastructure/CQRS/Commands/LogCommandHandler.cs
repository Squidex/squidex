// ==========================================================================
//  LogExecutingHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class LogCommandHandler : ICommandHandler
    {
        private readonly ISemanticLog log;

        public LogCommandHandler(ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));

            this.log = log;
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            try
            {
                log.LogInformation(w => w
                    .WriteProperty("action", "HandleCommand.")
                    .WriteProperty("actionId", context.ContextId.ToString())
                    .WriteProperty("state", "Started")
                    .WriteProperty("commandType", context.Command.GetType().Name));

                using (log.MeasureInformation(w => w
                    .WriteProperty("action", "HandleCommand.")
                    .WriteProperty("actionId", context.ContextId.ToString())
                    .WriteProperty("state", "Completed")
                    .WriteProperty("commandType", context.Command.GetType().Name)))
                {
                    await next();
                }

                log.LogInformation(w => w
                    .WriteProperty("action", "HandleCommand.")
                    .WriteProperty("actionId", context.ContextId.ToString())
                    .WriteProperty("state", "Succeeded")
                    .WriteProperty("commandType", context.Command.GetType().Name));
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "HandleCommand.")
                    .WriteProperty("actionId", context.ContextId.ToString())
                    .WriteProperty("state", "Failed")
                    .WriteProperty("commandType", context.Command.GetType().Name));

                throw;
            }

            if (!context.IsCompleted)
            {
                log.LogFatal(w => w
                    .WriteProperty("action", "HandleCommand.")
                    .WriteProperty("actionId", context.ContextId.ToString())
                    .WriteProperty("state", "Unhandled")
                    .WriteProperty("commandType", context.Command.GetType().Name));
            }
        }
    }
}
