// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public abstract class QueryExecutionContext : Dictionary<string, object?>
{
    private readonly SemaphoreSlim maxRequests = new SemaphoreSlim(10);

    public abstract Context Context { get; }

    protected IAssetQueryService AssetQuery { get; }

    protected IAssetCache AssetCache { get; }

    protected IContentCache ContentCache { get; }

    protected IContentQueryService ContentQuery { get; }

    public IServiceProvider Services { get; }

    protected QueryExecutionContext(
        IAssetQueryService assetQuery,
        IAssetCache assetCache,
        IContentQueryService contentQuery,
        IContentCache contentCache,
        IServiceProvider serviceProvider)
    {
        Guard.NotNull(serviceProvider);

        AssetQuery = assetQuery;
        AssetCache = assetCache;
        ContentQuery = contentQuery;
        ContentCache = contentCache;

        Services = serviceProvider;
    }

    public virtual Task<EnrichedContent?> FindContentAsync(string schemaIdOrName, DomainId id, long version,
        CancellationToken ct)
    {
        return ContentQuery.FindAsync(Context, schemaIdOrName, id, version, ct);
    }

    public virtual async Task<IResultList<EnrichedAsset>> QueryAssetsAsync(Q q,
        CancellationToken ct)
    {
        IResultList<EnrichedAsset> assets;

        await maxRequests.WaitAsync(ct);
        try
        {
            assets = await AssetQuery.QueryAsync(Context, null, q, ct);
        }
        finally
        {
            maxRequests.Release();
        }

        return assets;
    }

    public virtual async Task<IResultList<EnrichedContent>> QueryContentsAsync(string schemaIdOrName, Q q,
        CancellationToken ct)
    {
        IResultList<EnrichedContent> contents;

        await maxRequests.WaitAsync(ct);
        try
        {
            contents = await ContentQuery.QueryAsync(Context, schemaIdOrName, q, ct);
        }
        finally
        {
            maxRequests.Release();
        }

        return contents;
    }

    public virtual async Task<IReadOnlyList<EnrichedAsset>> QueryAssetsByIdsAsync(IEnumerable<DomainId> ids,
        CancellationToken ct)
    {
        Guard.NotNull(ids);

        await maxRequests.WaitAsync(ct);
        try
        {
            var q = Q.Empty.WithIds(ids).WithoutTotal();

            return await AssetQuery.QueryAsync(Context, null, q, ct);
        }
        finally
        {
            maxRequests.Release();
        }
    }

    public virtual async Task<IReadOnlyList<EnrichedContent>> QueryContentsByIdsAsync(IEnumerable<DomainId> ids, HashSet<string>? fields,
        CancellationToken ct)
    {
        Guard.NotNull(ids);

        await maxRequests.WaitAsync(ct);
        try
        {
            var q = Q.Empty.WithIds(ids).WithFields(fields).WithoutTotal();

            return await ContentQuery.QueryAsync(Context, q, ct);
        }
        finally
        {
            maxRequests.Release();
        }
    }

    public T Resolve<T>() where T : class
    {
        var key = typeof(T).Name;

        if (TryGetValue(key, out var stored) && stored is T typed)
        {
            return typed;
        }

        typed = Services.GetRequiredService<T>();

        this[key] = typed;

        return typed;
    }
}
