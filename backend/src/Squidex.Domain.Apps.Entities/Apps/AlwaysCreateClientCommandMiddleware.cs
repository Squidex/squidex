// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Apps;

public sealed class AlwaysCreateClientCommandMiddleware : ICommandMiddleware
{
    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        await next(context, ct);

        if (context.IsCompleted && context.Command is CreateApp createApp)
        {
            var appId = NamedId.Of(createApp.AppId, createApp.Name);

            var publish = new Func<IAppCommand, Task>(async command =>
            {
                command.AppId = appId;

                // If we have the app already it is not worth to cancel the step here.
                var newContext = await context.CommandBus.PublishAsync(command, default);

                context.Complete(newContext.PlainResult);
            });

            await publish(new AttachClient { Id = "default", Role = Role.Owner });
        }
    }
}
