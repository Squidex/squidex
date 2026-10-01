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
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json;
using Squidex.Shared;

#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public sealed class CachingContentQueryService(
    IContentQueryService inner,
    HybridCache cache,
    ICacheGenerations generations,
    IContentEnricher contentEnricher,
    IJsonSerializer serializer,
    IOptions<ContentQueryCacheOptions> options,
    ILogger<CachingContentQueryService> log)
    : IContentQueryService, ICommandMiddleware
{
    private sealed record CachedContents(long Total, EnrichedContent[] Items);

    private sealed record CacheKey(string Generation, long AppVersion, DomainId SchemaId, long SchemaVersion, IReadOnlyDictionary<string, string> Headers, Q Query);

    // Only the distributed cache is used, because the local caches of the other nodes cannot be invalidated.
    private readonly HybridCacheEntryOptions entryOptions = new HybridCacheEntryOptions
    {
        Expiration = options.Value.CacheDuration,
        Flags = HybridCacheEntryFlags.DisableLocalCache,
    };

    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        await next(context, ct);

        if (context.Command is not IAppCommand appCommand || !ChangesContents(appCommand))
        {
            return;
        }

        static bool ChangesContents(ICommand command)
        {
            // Queries remove references to deleted assets.
            return command is ContentCommand and not ValidateContent and not EnrichContentDefaults or DeleteAsset;
        }

        // A new generation changes all cache keys of the app on all nodes.
        generations.Reset(GenerationKey(appCommand.AppId.Id));
    }

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

        var key = await CreateKeyAsync(context, schema, q, ct);
        if (key == null)
        {
            return await inner.QueryAsync(context, schemaIdOrName, q, ct);
        }

        var isQueried = false;

        var cached = await cache.GetOrCreateAsync(key, async ct =>
        {
            isQueried = true;

            var contents = await inner.QueryAsync(context, schemaIdOrName, q, ct);

            return new CachedContents(contents.Total, contents.ToArray());
        }, entryOptions, cancellationToken: ct);

        // Results from the cache have skipped the enrichment steps, which only affect the current request.
        if (!isQueried)
        {
            await contentEnricher.EnrichCachedAsync(cached.Items, context, ct);
        }

        return ResultList.Create(cached.Total, cached.Items);
    }

    public Task<EnrichedContent?> FindAsync(Context context, string schemaIdOrName, DomainId id, long version = EtagVersion.Any,
        CancellationToken ct = default)
    {
        // Single contents are loaded by ID, which is not faster with the distributed cache.
        return inner.FindAsync(context, schemaIdOrName, id, version, ct);
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

    private async Task<string?> CreateKeyAsync(Context context, Schema schema, Q q,
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

        var keyObj = new CacheKey(generation, context.App.Version, schema.Id, schema.Version, context.Headers, q);
        var keyHash = SHA256.HashData(serializer.SerializeToBytes(keyObj));

        return $"contents/{context.App.Id}/{Convert.ToHexString(keyHash)}";
    }

    private static bool CanCache(Context context, Schema schema)
    {
        // Clients can bypass the cache, for example with the Cache-Control header.
        if (context.NoQueryCache())
        {
            return false;
        }

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

    private static string GenerationKey(DomainId appId)
    {
        return $"contents/{appId}";
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
