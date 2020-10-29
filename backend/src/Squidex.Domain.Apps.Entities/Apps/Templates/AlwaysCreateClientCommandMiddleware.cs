// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    public sealed class AlwaysCreateClientCommandMiddleware : ICommandMiddleware
    {
        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            await next(context);

            if (context.IsCompleted && context.Command is CreateApp createApp)
            {
                var appId = NamedId.Of(createApp.AppId, createApp.Name);

                var command = new AttachClient { Id = "default", AppId = appId };

                await context.CommandBus.PublishAsync(command);
            }
        }
    }
}
