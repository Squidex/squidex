// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web.CommandMiddlewares;

public sealed class EnrichWithAppIdCommandMiddleware : ICommandMiddleware
{
    private readonly IContextProvider contextProvider;

    public EnrichWithAppIdCommandMiddleware(IContextProvider contextProvider)
    {
        this.contextProvider = contextProvider;
    }

    public Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        if (context.Command is IAppCommand { AppId: null } appCommand)
        {
            var appId = GetAppId();

            appCommand.AppId = appId;
        }

        return next(context, ct);
    }

    private NamedId<DomainId> GetAppId()
    {
        var context = contextProvider.Context;

        if (context.App == null)
        {
            ThrowHelper.InvalidOperationException("Cannot resolve app.");
            return default!;
        }

        return context.App.NamedId();
    }
}
