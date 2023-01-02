// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure.Caching;

namespace Squidex.Domain.Apps.Entities.Assets.Queries.Steps;

public sealed class EnrichForCaching : IAssetEnricherStep
{
    private readonly IRequestCache requestCache;

    public EnrichForCaching(IRequestCache requestCache)
    {
        this.requestCache = requestCache;
    }

    public Task EnrichAsync(Context context,
        CancellationToken ct)
    {
        context.AddCacheHeaders(requestCache);

        return Task.CompletedTask;
    }

    public Task EnrichAsync(Context context, IEnumerable<AssetEntity> assets,
        CancellationToken ct)
    {
        requestCache.AddDependency(context.App.Id, context.App.Version);

        foreach (var asset in assets)
        {
            requestCache.AddDependency(asset.UniqueId, asset.Version);
        }

        return Task.CompletedTask;
    }
}
