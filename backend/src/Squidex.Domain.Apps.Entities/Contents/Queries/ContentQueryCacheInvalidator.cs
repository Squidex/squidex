// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public sealed class ContentQueryCacheInvalidator(ICacheGenerations generations) : ICommandMiddleware
{
    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        await next(context, ct);

        if (context.Command is not IAppCommand appCommand || !ChangesContents(appCommand))
        {
            return;
        }

        // A new generation changes all cache keys of the app on all nodes.
        var appId = appCommand.AppId.Id;

        await generations.ResetAsync(CachingContentQueryService.GenerationKey(appId), default);
    }

    private static bool ChangesContents(ICommand command)
    {
        // Queries remove references to deleted assets.
        switch (command)
        {
            case ValidateContent:
            case EnrichContentDefaults:
                return false;
            case ContentCommand:
                return true;
            case DeleteAsset:
                return true;
            default:
                return false;
        }
    }
}
