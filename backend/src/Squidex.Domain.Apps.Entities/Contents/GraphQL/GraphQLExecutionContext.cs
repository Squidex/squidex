// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.DataLoader;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Cache;
using Squidex.Domain.Apps.Entities.Contents.Queries;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public sealed class GraphQLExecutionContext : QueryExecutionContext
{
    private const int MaxBatchSize = 5000;
    private const int MinBatchSize = 1;
    private static readonly EmptyDataLoaderResult<EnrichedAsset> EmptyAssets = new EmptyDataLoaderResult<EnrichedAsset>();
    private static readonly EmptyDataLoaderResult<EnrichedContent> EmptyContents = new EmptyDataLoaderResult<EnrichedContent>();
    private readonly IDataLoaderContextAccessor dataLoaders;
    private readonly GraphQLOptions options;
    private readonly int batchSize;

    public override Context Context { get; }

    public GraphQLExecutionContext(
        IDataLoaderContextAccessor dataLoaders,
        IAssetQueryService assetQuery,
        IAssetCache assetCache,
        IContentQueryService contentQuery,
        IContentCache contentCache,
        IServiceProvider serviceProvider,
        Context context,
        IOptions<GraphQLOptions> options)
        : base(assetQuery, assetCache, contentQuery, contentCache, serviceProvider)
    {
        this.dataLoaders = dataLoaders;

        Context = context.Clone(b => b
            .WithResolveSchemaNames()
            .WithNoCleanup()
            .WithNoEnrichment());

        this.options = options.Value;

        batchSize = Context.BatchSize();

        if (batchSize == 0)
        {
            batchSize = options.Value.DataLoaderBatchSize;
        }
        else
        {
            batchSize = Math.Max(MinBatchSize, Math.Min(MaxBatchSize, batchSize));
        }
    }

    public async ValueTask<IUser?> FindUserAsync(RefToken refToken,
        CancellationToken ct)
    {
        if (refToken.IsClient)
        {
            return new ClientUser(refToken);
        }
        else
        {
            var dataLoader = GetUserLoader();

            return await dataLoader.LoadAsync(refToken.Identifier).GetResultAsync(ct);
        }
    }

    public IDataLoaderResult<EnrichedContent?> GetContent(DomainId schemaId, DomainId id, long version)
    {
        var cacheKey = $"{nameof(GetContent)}_{schemaId}_{id}_{version}";

        return dataLoaders.Context!.GetOrAddLoader(cacheKey, ct =>
        {
            return FindContentAsync(schemaId.ToString(), id, version, ct);
        }).LoadAsync();
    }

    public IDataLoaderResult<EnrichedAsset?> GetAsset(DomainId id,
        TimeSpan cacheDuration)
    {
        var assets = GetAssets([id], cacheDuration);
        var asset = assets.Then(x => x.FirstOrDefault());

        return asset;
    }

    public IDataLoaderResult<EnrichedContent?> GetContent(DomainId schemaId, DomainId id, HashSet<string>? fields,
        TimeSpan cacheDuration)
    {
        var contents = GetContents([id], fields, cacheDuration);
        var content = contents.Then(x => x.FirstOrDefault(x => x.SchemaId.Id == schemaId));

        return content;
    }

    public IDataLoaderResult<EnrichedAsset[]> GetAssets(List<DomainId>? ids,
        TimeSpan cacheDuration)
    {
        if (ids is not { Count: > 0 })
        {
            return EmptyAssets;
        }

        return GetAssetsLoader().LoadAsync(BuildKeys(ids, cacheDuration)).Then(x => x.NotNull().ToArray());
    }

    public IDataLoaderResult<EnrichedContent[]> GetContents(List<DomainId>? ids, HashSet<string>? fields,
        TimeSpan cacheDuration)
    {
        if (ids is not { Count: > 0 })
        {
            return EmptyContents;
        }

        if (fields == null)
        {
            return GetContentsLoader().LoadAsync(BuildKeys(ids, cacheDuration)).Then(x => x.NotNull().ToArray());
        }

        return GetContentsLoaderWithFields().LoadAsync(BuildKeys(ids, fields)).Then(x => x.NotNull().ToArray());
    }

    private IDataLoader<CacheableId<DomainId>, EnrichedAsset> GetAssetsLoader()
    {
        return dataLoaders.Context!.GetOrAddCachingLoader(AssetCache, nameof(GetAssetsLoader),
            async (batch, ct) =>
            {
                var result = await QueryAssetsByIdsAsync(batch, ct);

                return result.ToDictionary(x => x.Id);
            }, maxBatchSize: batchSize);
    }

    private IDataLoader<CacheableId<DomainId>, EnrichedContent> GetContentsLoader()
    {
        return dataLoaders.Context!.GetOrAddCachingLoader(ContentCache, nameof(GetContentsLoader),
            async (batch, ct) =>
            {
                var result = await QueryContentsByIdsAsync(batch, null, ct);

                return result.ToDictionary(x => x.Id);
            }, maxBatchSize: batchSize);
    }

    private IDataLoader<(DomainId Id, HashSet<string> Fields), EnrichedContent> GetContentsLoaderWithFields()
    {
        return dataLoaders.Context!.GetOrAddNonCachingBatchLoader<(DomainId Id, HashSet<string> Fields), EnrichedContent>(nameof(GetContentsLoaderWithFields),
            async (batch, ct) =>
            {
                var fields = batch.SelectMany(x => x.Fields).ToHashSet();

                var result = await QueryContentsByIdsAsync(batch.Select(x => x.Id), fields, ct);

                return result.ToDictionary(x => (x.Id, fields));
            }, maxBatchSize: batchSize);
    }

    private IDataLoader<string, IUser> GetUserLoader()
    {
        return dataLoaders.Context!.GetOrAddBatchLoader<string, IUser>(nameof(GetUserLoader),
            async (batch, ct) =>
            {
                var result = await Resolve<IUserResolver>().QueryManyAsync(batch.ToArray(), ct);

                return result;
            });
    }

    private static (DomainId, HashSet<string>)[] BuildKeys(List<DomainId> ids, HashSet<string> fields)
    {
        // Use manual loops and arrays to avoid allocations.
        var keys = new (DomainId, HashSet<string>)[ids.Count];

        for (var i = 0; i < ids.Count; i++)
        {
            keys[i] = (ids[0], fields);
        }

        return keys;
    }

    private static CacheableId<DomainId>[] BuildKeys(List<DomainId> ids, TimeSpan cacheDuration)
    {
        // Use manual loops and arrays to avoid allocations.
        var keys = new CacheableId<DomainId>[ids.Count];

        for (var i = 0; i < ids.Count; i++)
        {
            keys[i] = new CacheableId<DomainId>(ids[i], cacheDuration);
        }

        return keys;
    }
}
