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
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json;

#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public sealed class CachingAssetQueryService(
    IAssetQueryService inner,
    HybridCache cache,
    ICacheGenerations generations,
    IAssetEnricher assetEnricher,
    IJsonSerializer serializer,
    IOptions<AssetQueryCacheOptions> options,
    ILogger<CachingAssetQueryService> log)
    : IAssetQueryService, ICommandMiddleware
{
    private sealed record CachedAssets(long Total, EnrichedAsset[] Items);

    private sealed record CacheKey(string Generation, long AppVersion, DomainId? ParentId, IReadOnlyDictionary<string, string> Headers, Q Query);

    private readonly HybridCacheEntryOptions entryOptions = new HybridCacheEntryOptions
    {
        Expiration = options.Value.CacheDuration,
        // Only the distributed cache is used, because the local caches of the other nodes cannot be invalidated.
        Flags = HybridCacheEntryFlags.DisableLocalCache,
    };

    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        await next(context, ct);

        if (context.Command is not IAppCommand appCommand || !ChangesAssets(appCommand))
        {
            return;
        }

        static bool ChangesAssets(ICommand command)
        {
            // Queries remove references to deleted assets.
            return command is AssetCommand;
        }

        // A new generation changes all cache keys of the app on all nodes.
        generations.Reset(GenerationKey(appCommand.AppId.Id));
    }

    public async Task<IResultList<EnrichedAsset>> QueryAsync(Context context, DomainId? parentId, Q q,
        CancellationToken ct = default)
    {
        Guard.NotNull(context);

        if (q == null || !CanCache(context) || !CanCache(q))
        {
            return await inner.QueryAsync(context, parentId, q!, ct);
        }

        var key = await CreateKeyAsync(context, parentId, q, ct);
        if (key == null)
        {
            return await inner.QueryAsync(context, parentId, q, ct);
        }

        var isQueried = false;

        var cached = await cache.GetOrCreateAsync(key, async ct =>
        {
            isQueried = true;

            var assets = await inner.QueryAsync(context, parentId, q, ct);

            return new CachedAssets(assets.Total, assets.ToArray());
        }, entryOptions, cancellationToken: ct);

        // Results from the cache have skipped the enrichment steps, which only affect the current request.
        if (!isQueried)
        {
            await assetEnricher.EnrichCachedAsync(cached.Items, context, ct);
        }

        return ResultList.Create(cached.Total, cached.Items);
    }

    public Task<EnrichedAsset?> FindAsync(Context context, DomainId id, bool allowDeleted = false, long version = EtagVersion.Any,
        CancellationToken ct = default)
    {
        return inner.FindAsync(context, id, allowDeleted, version, ct);
    }

    public Task<EnrichedAsset?> FindBySlugAsync(Context context, string slug, bool allowDeleted = false,
        CancellationToken ct = default)
    {
        return inner.FindBySlugAsync(context, slug, allowDeleted, ct);
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
        return inner.FindByHashAsync(context, hash, fileName, fileSize, ct);
    }

    public Task<EnrichedAsset?> FindGlobalAsync(Context context, DomainId id,
        CancellationToken ct = default)
    {
        return inner.FindGlobalAsync(context, id, ct);
    }

    private static string GenerationKey(DomainId appId)
    {
        return $"assets/{appId}";
    }

    private async Task<string?> CreateKeyAsync(Context context, DomainId? parentId, Q q,
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

        var keyObj = new CacheKey(generation, context.App.Version, parentId, context.Headers, q);
        var keyHash = SHA256.HashData(serializer.SerializeToBytes(keyObj));

        return $"assets/{context.App.Id}/{Convert.ToHexString(keyHash)}";
    }

    private static bool CanCache(Context context)
    {
        // Clients can bypass the cache, for example with the Cache-Control header.
        if (context.NoQueryCache())
        {
            return false;
        }

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
