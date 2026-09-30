// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Json;

#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public sealed class CachingAssetQueryService(
    IAssetQueryService inner,
    HybridCache cache,
    ICacheGenerations generations,
    IRequestCache requestCache,
    IJsonSerializer serializer,
    IOptions<AssetQueryCacheOptions> options,
    ILogger<CachingAssetQueryService> log)
    : IAssetQueryService
{
    private sealed record CachedAssets(long Total, EnrichedAsset[] Items);

    private sealed record CacheKey<T>(string Kind, string Generation, long AppVersion, IReadOnlyDictionary<string, string> Headers, T Request);

    // Only the distributed cache is used, because the local caches of the other nodes cannot be invalidated.
    private readonly HybridCacheEntryOptions entryOptions = new HybridCacheEntryOptions
    {
        Expiration = options.Value.CacheDuration,
        Flags = HybridCacheEntryFlags.DisableLocalCache,
    };

    public async Task<IResultList<EnrichedAsset>> QueryAsync(Context context, DomainId? parentId, Q q,
        CancellationToken ct = default)
    {
        Guard.NotNull(context);

        if (q == null || !CanCache(context) || !CanCache(q))
        {
            return await inner.QueryAsync(context, parentId, q!, ct);
        }

        var cached = await GetOrQueryAsync(context, "assets.query", new { parentId, q }, async ct =>
        {
            var assets = await inner.QueryAsync(context, parentId, q, ct);

            return new CachedAssets(assets.Total, assets.ToArray());
        }, ct);

        return ResultList.Create(cached.Total, cached.Items);
    }

    public async Task<EnrichedAsset?> FindAsync(Context context, DomainId id, bool allowDeleted = false, long version = EtagVersion.Any,
        CancellationToken ct = default)
    {
        Guard.NotNull(context);

        // Specific versions are loaded from the event store and not from the query store.
        if (version != EtagVersion.Any || !CanCache(context))
        {
            return await inner.FindAsync(context, id, allowDeleted, version, ct);
        }

        var cached = await GetOrQueryAsync(context, "assets.find", new { id, allowDeleted }, async ct =>
        {
            return Single(await inner.FindAsync(context, id, allowDeleted, version, ct));
        }, ct);

        return cached.Items.FirstOrDefault();
    }

    public async Task<EnrichedAsset?> FindBySlugAsync(Context context, string slug, bool allowDeleted = false,
        CancellationToken ct = default)
    {
        Guard.NotNull(context);

        if (!CanCache(context))
        {
            return await inner.FindBySlugAsync(context, slug, allowDeleted, ct);
        }

        var cached = await GetOrQueryAsync(context, "assets.slug", new { slug, allowDeleted }, async ct =>
        {
            return Single(await inner.FindBySlugAsync(context, slug, allowDeleted, ct));
        }, ct);

        return cached.Items.FirstOrDefault();
    }

    public Task<IResultList<AssetFolder>> QueryAssetFoldersAsync(Context context, DomainId? parentId,
        CancellationToken ct = default)
    {
        return inner.QueryAssetFoldersAsync(context, parentId, ct);
    }

    public Task<IReadOnlyList<AssetFolder>> FindAssetFolderAsync(DomainId appId, DomainId id,
        CancellationToken ct = default)
    {
        return inner.FindAssetFolderAsync(appId, id, ct);
    }

    public Task<EnrichedAsset?> FindByHashAsync(Context context, string hash, string fileName, long fileSize,
        CancellationToken ct = default)
    {
        // Used to detect duplicates when uploading and therefore needs the latest state.
        return inner.FindByHashAsync(context, hash, fileName, fileSize, ct);
    }

    public Task<EnrichedAsset?> FindGlobalAsync(Context context, DomainId id,
        CancellationToken ct = default)
    {
        // The asset can belong to any app, therefore there is no generation to invalidate it.
        return inner.FindGlobalAsync(context, id, ct);
    }

    public static string GenerationKey(DomainId appId)
    {
        return $"assets/{appId}";
    }

    private async Task<CachedAssets> GetOrQueryAsync<TRequest>(Context context, string kind, TRequest request, Func<CancellationToken, Task<CachedAssets>> query,
        CancellationToken ct)
    {
        var key = await CreateKeyAsync(context, kind, request, ct);
        if (key == null)
        {
            return await query(ct);
        }

        var cached = await cache.GetOrCreateAsync(key, async ct => await query(ct), entryOptions, cancellationToken: ct);

        AddCacheDependencies(context, cached.Items);

        return cached;
    }

    private async Task<string?> CreateKeyAsync<TRequest>(Context context, string kind, TRequest request,
        CancellationToken ct)
    {
        string generation;
        try
        {
            generation = await generations.GetAsync(GenerationKey(context.App.Id), ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Without the generation the cache cannot be used, but the query must not fail because of it.
            log.LogWarning(ex, "Failed to read asset query cache generation for app {appId}.", context.App.Id);
            return null;
        }

        var key = new CacheKey<TRequest>(kind, generation, context.App.Version, context.Headers, request);
        var keyHash = SHA256.HashData(serializer.SerializeToBytes(key));

        return $"{kind}/{context.App.Id}/{Convert.ToHexString(keyHash)}";
    }

    private void AddCacheDependencies(Context context, EnrichedAsset[] assets)
    {
        // A cache hit skips the enricher, which usually adds the dependencies for the etag and the surrogate keys.
        if (context.NoCacheKeys())
        {
            return;
        }

        context.AddCacheHeaders(requestCache);

        requestCache.AddDependency(context.App.Id, context.App.Version);

        foreach (var asset in assets)
        {
            requestCache.AddDependency(asset.UniqueId, asset.Version);
        }
    }

    private static CachedAssets Single(EnrichedAsset? asset)
    {
        return asset != null ? new CachedAssets(1, [asset]) : new CachedAssets(0, []);
    }

    private static bool CanCache(Context context)
    {
        // The UI always needs the latest state.
        if (context.IsFrontendClient)
        {
            return false;
        }

        // Scripts can depend on the user or on the current time.
        if (!context.NoScripting() && HasQueryScripts(context))
        {
            return false;
        }

        return true;
    }

    private static bool CanCache(Q q)
    {
        // The parsed query is usually only set by the query parser and cannot be serialized for the cache key.
        if (!string.IsNullOrEmpty(q.Query?.ToString()))
        {
            return false;
        }

        // Random queries should return a different result for each call.
        if (q.JsonQuery?.Random > 0)
        {
            return false;
        }

        return !ContainsRandom(q.QueryAsJson) && !ContainsRandom(q.QueryAsOdata);
    }

    private static bool ContainsRandom(string? query)
    {
        return query?.Contains("random", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static bool HasQueryScripts(Context context)
    {
        return !string.IsNullOrWhiteSpace(context.App.AssetScripts.Query) || !string.IsNullOrWhiteSpace(context.App.AssetScripts.QueryPre);
    }
}
