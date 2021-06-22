// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Log;

namespace Squidex.Infrastructure.Commands
{
    public sealed class LogCommandMiddleware : ICommandMiddleware
    {
        private readonly ISemanticLog log;

        public LogCommandMiddleware(ISemanticLog log)
        {
            this.log = log;
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            var logContext = (id: context.ContextId.ToString(), command: context.Command.GetType().Name);

            try
            {
                log.LogDebug(logContext, (ctx, w) => w
                    .WriteProperty("action", "HandleCommand.")
                    .WriteProperty("actionId", ctx.id)
                    .WriteProperty("status", "Started")
                    .WriteProperty("commandType", ctx.command));

                using (log.MeasureInformation(logContext, (ctx, w) => w
                    .WriteProperty("action", "HandleCommand.")
                    .WriteProperty("actionId", ctx.id)
                    .WriteProperty("status", "Completed")
                    .WriteProperty("commandType", ctx.command)))
                {
                    await next(context);
                }

                log.LogInformation(logContext, (ctx, w) => w
                    .WriteProperty("action", "HandleCommand.")
                    .WriteProperty("actionId", ctx.id)
                    .WriteProperty("status", "Succeeded")
                    .WriteProperty("commandType", ctx.command));
            }
            catch (Exception ex)
            {
                log.LogError(ex, logContext, (ctx, w) => w
                    .WriteProperty("action", "HandleCommand.")
                    .WriteProperty("actionId", ctx.id)
                    .WriteProperty("status", "Failed")
                    .WriteProperty("commandType", ctx.command));

                throw;
            }

            if (!context.IsCompleted)
            {
                log.LogFatal(logContext, (ctx, w) => w
                    .WriteProperty("action", "HandleCommand.")
                    .WriteProperty("actionId", ctx.id)
                    .WriteProperty("status", "Unhandled")
                    .WriteProperty("commandType", ctx.command));
            }
        }
    }
}
