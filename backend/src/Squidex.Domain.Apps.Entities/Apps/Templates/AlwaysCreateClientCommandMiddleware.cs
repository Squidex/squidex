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
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    public sealed class AlwaysCreateClientCommandMiddleware : ICommandMiddleware
    {
        public Task HandleAsync(CommandContext context, NextDelegate next)
        {
            if (context.IsCompleted && context.Command is CreateApp createApp)
            {
                var appId = NamedId.Of(createApp.AppId, createApp.Name);

                var command = new AttachClient { Id = "default", AppId = appId };

                context.CommandBus.PublishAsync(command).Forget();
            }

            return next(context);
        }
    }
}
