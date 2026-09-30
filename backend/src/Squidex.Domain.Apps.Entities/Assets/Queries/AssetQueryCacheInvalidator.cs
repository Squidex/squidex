// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public sealed class AssetQueryCacheInvalidator(ICacheGenerations generations) : ICommandMiddleware
{
    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        await next(context, ct);

        if (context.Command is not AssetCommand asset)
        {
            return;
        }

        // A new generation changes all cache keys of the app on all nodes.
        await generations.ResetAsync(CachingAssetQueryService.GenerationKey(asset.AppId.Id), default);
    }
}
