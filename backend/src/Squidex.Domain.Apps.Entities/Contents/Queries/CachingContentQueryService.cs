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
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Json;
using Squidex.Shared;

#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public sealed class CachingContentQueryService(
    IContentQueryService inner,
    HybridCache cache,
    ICacheGenerations generations,
    IRequestCache requestCache,
    IJsonSerializer serializer,
    IOptions<ContentQueryCacheOptions> options,
    ILogger<CachingContentQueryService> log)
    : IContentQueryService
{
    private sealed record CachedContents(long Total, EnrichedContent[] Items);

    private sealed record CacheKey<T>(string Kind, string Generation, long AppVersion, IReadOnlyDictionary<string, string> Headers, T Request);

    // Only the distributed cache is used, because the local caches of the other nodes cannot be invalidated.
    private readonly HybridCacheEntryOptions entryOptions = new HybridCacheEntryOptions
    {
        Expiration = options.Value.CacheDuration,
        Flags = HybridCacheEntryFlags.DisableLocalCache,
    };

    public IAsyncEnumerable<EnrichedContent> StreamAsync(Context context, string schemaIdOrName, int skip,
        CancellationToken ct = default)
    {
        return inner.StreamAsync(context, schemaIdOrName, skip, ct);
    }

    public Task<IResultList<EnrichedContent>> QueryAsync(Context context, Q q,
        CancellationToken ct = default)
    {
        // Queries over all schemas depend on the schema permissions of the user, therefore they are not cached.
        return inner.QueryAsync(context, q, ct);
    }

    public async Task<IResultList<EnrichedContent>> QueryAsync(Context context, string schemaIdOrName, Q q,
        CancellationToken ct = default)
    {
        Guard.NotNull(context);

        if (q == null)
        {
            return await inner.QueryAsync(context, schemaIdOrName, q!, ct);
        }

        // Also validates the permissions, which must never be skipped by a cache hit.
        var schema = await inner.GetSchemaOrThrowAsync(context, schemaIdOrName, ct);
        if (!CanCache(context, schema) || !CanCache(q))
        {
            return await inner.QueryAsync(context, schemaIdOrName, q, ct);
        }

        var cached = await GetOrQueryAsync(context, schema, "contents.query", q, async ct =>
        {
            var contents = await inner.QueryAsync(context, schemaIdOrName, q, ct);

            return new CachedContents(contents.Total, contents.ToArray());
        }, ct);

        return ResultList.Create(cached.Total, cached.Items);
    }

    public async Task<EnrichedContent?> FindAsync(Context context, string schemaIdOrName, DomainId id, long version = EtagVersion.Any,
        CancellationToken ct = default)
    {
        Guard.NotNull(context);

        // Specific versions are loaded from the event store and not from the query store.
        if (version != EtagVersion.Any)
        {
            return await inner.FindAsync(context, schemaIdOrName, id, version, ct);
        }

        // Also validates the permissions, which must never be skipped by a cache hit.
        var schema = await inner.GetSchemaOrThrowAsync(context, schemaIdOrName, ct);
        if (!CanCache(context, schema))
        {
            return await inner.FindAsync(context, schemaIdOrName, id, version, ct);
        }

        var cached = await GetOrQueryAsync(context, schema, "contents.find", id, async ct =>
        {
            var content = await inner.FindAsync(context, schemaIdOrName, id, version, ct);

            return content != null ? new CachedContents(1, [content]) : new CachedContents(0, []);
        }, ct);

        return cached.Items.FirstOrDefault();
    }

    public Task<Schema> GetSchemaOrThrowAsync(Context context, string schemaIdOrName,
        CancellationToken ct = default)
    {
        return inner.GetSchemaOrThrowAsync(context, schemaIdOrName, ct);
    }

    public Task<Schema?> GetSchemaAsync(Context context, string schemaIdOrName,
        CancellationToken ct = default)
    {
        return inner.GetSchemaAsync(context, schemaIdOrName, ct);
    }

    public static string GenerationKey(DomainId appId)
    {
        return $"contents/{appId}";
    }

    private async Task<CachedContents> GetOrQueryAsync<TRequest>(Context context, Schema schema, string kind, TRequest request, Func<CancellationToken, Task<CachedContents>> query,
        CancellationToken ct)
    {
        var key = await CreateKeyAsync(context, kind, new { schemaId = schema.Id, schemaVersion = schema.Version, request }, ct);
        if (key == null)
        {
            return await query(ct);
        }

        var cached = await cache.GetOrCreateAsync(key, async ct => await query(ct), entryOptions, cancellationToken: ct);

        AddCacheDependencies(context, schema, cached.Items);

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
            log.LogWarning(ex, "Failed to read content query cache generation for app {appId}.", context.App.Id);
            return null;
        }

        var key = new CacheKey<TRequest>(kind, generation, context.App.Version, context.Headers, request);
        var keyHash = SHA256.HashData(serializer.SerializeToBytes(key));

        return $"{kind}/{context.App.Id}/{Convert.ToHexString(keyHash)}";
    }

    private void AddCacheDependencies(Context context, Schema schema, EnrichedContent[] contents)
    {
        // A cache hit skips the enricher, which usually adds the dependencies for the etag and the surrogate keys.
        if (context.NoCacheKeys())
        {
            return;
        }

        context.AddCacheHeaders(requestCache);

        if (contents.Length == 0)
        {
            return;
        }

        requestCache.AddDependency(context.App.UniqueId, context.App.Version);
        requestCache.AddDependency(schema.UniqueId, schema.Version);

        foreach (var content in contents)
        {
            requestCache.AddDependency(content.UniqueId, content.Version);
        }
    }

    private static bool CanCache(Context context, Schema schema)
    {
        // Only published content is invalidated reliably and the UI always needs the latest state.
        if (context.IsFrontendClient || context.Scope() != SearchScope.Published)
        {
            return false;
        }

        // The workflow information depends on the current user.
        if (context.ResolveFlow())
        {
            return false;
        }

        // With the read.own permission the result depends on the current user.
        if (!context.Allows(PermissionIds.AppContentsRead, schema.Name))
        {
            return false;
        }

        // Scripts can depend on the user or on the current time.
        if (!context.NoScripting() && HasQueryScripts(schema))
        {
            return false;
        }

        return true;
    }

    private static bool CanCache(Q q)
    {
        // The schedule queries are made for background processes.
        if (q.ScheduledFrom != null || q.ScheduledTo != null)
        {
            return false;
        }

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

    private static bool HasQueryScripts(Schema schema)
    {
        return !string.IsNullOrWhiteSpace(schema.Scripts.Query) || !string.IsNullOrWhiteSpace(schema.Scripts.QueryPre);
    }
}
