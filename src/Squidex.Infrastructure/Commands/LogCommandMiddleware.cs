// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.Commands
{
    public sealed class LogCommandMiddleware : ICommandMiddleware
    {
        private readonly ISemanticLog log;

        public LogCommandMiddleware(ISemanticLog log)
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
                    .WriteProperty("status", "Started")
                    .WriteProperty("commandType", context.Command.GetType().Name));

                using (log.MeasureInformation(w => w
                    .WriteProperty("action", "HandleCommand.")
                    .WriteProperty("actionId", context.ContextId.ToString())
                    .WriteProperty("status", "Completed")
                    .WriteProperty("commandType", context.Command.GetType().Name)))
                {
                    await next();
                }

                log.LogInformation(w => w
                    .WriteProperty("action", "HandleCommand.")
                    .WriteProperty("actionId", context.ContextId.ToString())
                    .WriteProperty("status", "Succeeded")
                    .WriteProperty("commandType", context.Command.GetType().Name));
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "HandleCommand.")
                    .WriteProperty("actionId", context.ContextId.ToString())
                    .WriteProperty("status", "Failed")
                    .WriteProperty("commandType", context.Command.GetType().Name));

                throw;
            }

            if (!context.IsCompleted)
            {
                log.LogFatal(w => w
                    .WriteProperty("action", "HandleCommand.")
                    .WriteProperty("actionId", context.ContextId.ToString())
                    .WriteProperty("status", "Unhandled")
                    .WriteProperty("commandType", context.Command.GetType().Name));
            }
        }
    }
}
