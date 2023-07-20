// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
    private static readonly EmptyDataLoaderResult<IEnrichedAssetEntity> EmptyAssets = new EmptyDataLoaderResult<IEnrichedAssetEntity>();
    private static readonly EmptyDataLoaderResult<IEnrichedContentEntity> EmptyContents = new EmptyDataLoaderResult<IEnrichedContentEntity>();
    private readonly IDataLoaderContextAccessor dataLoaders;
    private readonly GraphQLOptions options;

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
            .WithoutCleanup()
            .WithoutContentEnrichment()
            .WithoutAssetEnrichment());

        this.options = options.Value;
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

    public IDataLoaderResult<IEnrichedContentEntity?> GetContent(DomainId schemaId, DomainId id, long version)
    {
        return dataLoaders.Context!.GetOrAddLoader(nameof(GetContent), ct =>
        {
            return FindContentAsync(schemaId.ToString(), id, version, ct);
        }).LoadAsync();
    }

    public IDataLoaderResult<IEnrichedAssetEntity?> GetAsset(DomainId id,
        TimeSpan cacheDuration)
    {
        var assets = GetAssets(new List<DomainId> { id }, cacheDuration);
        var asset = assets.Then(x => x.FirstOrDefault());

        return asset;
    }

    public IDataLoaderResult<IEnrichedContentEntity?> GetContent(DomainId schemaId, DomainId id, HashSet<string>? fields,
        TimeSpan cacheDuration)
    {
        var contents = GetContents(new List<DomainId> { id }, fields, cacheDuration);
        var content = contents.Then(x => x.FirstOrDefault(x => x.SchemaId.Id == schemaId));

        return content;
    }

    public IDataLoaderResult<IEnrichedAssetEntity[]> GetAssets(List<DomainId>? ids,
        TimeSpan cacheDuration)
    {
        if (ids == null || ids.Count == 0)
        {
            return EmptyAssets;
        }

        return GetAssetsLoader().LoadAsync(BuildKeys(ids, cacheDuration)).Then(x => x.NotNull().ToArray());
    }

    public IDataLoaderResult<IEnrichedContentEntity[]> GetContents(List<DomainId>? ids, HashSet<string>? fields,
        TimeSpan cacheDuration)
    {
        if (ids == null || ids.Count == 0)
        {
            return EmptyContents;
        }

        if (fields == null)
        {
            return GetContentsLoader().LoadAsync(BuildKeys(ids, cacheDuration)).Then(x => x.NotNull().ToArray());
        }

        return GetContentsLoaderWithFields().LoadAsync(BuildKeys(ids, fields)).Then(x => x.NotNull().ToArray());
    }

    private IDataLoader<CacheableId<DomainId>, IEnrichedAssetEntity> GetAssetsLoader()
    {
        return dataLoaders.Context!.GetOrAddCachingLoader(AssetCache, nameof(GetAssetsLoader),
            async (batch, ct) =>
            {
                var result = await QueryAssetsByIdsAsync(batch, ct);

                return result.ToDictionary(x => x.Id);
            }, maxBatchSize: options.DataLoaderBatchSize);
    }

    private IDataLoader<CacheableId<DomainId>, IEnrichedContentEntity> GetContentsLoader()
    {
        return dataLoaders.Context!.GetOrAddCachingLoader(ContentCache, nameof(GetContentsLoader),
            async (batch, ct) =>
            {
                var result = await QueryContentsByIdsAsync(batch, null, ct);

                return result.ToDictionary(x => x.Id);
            }, maxBatchSize: options.DataLoaderBatchSize);
    }

    private IDataLoader<(DomainId Id, HashSet<string> Fields), IEnrichedContentEntity> GetContentsLoaderWithFields()
    {
        return dataLoaders.Context!.GetOrAddNonCachingBatchLoader<(DomainId Id, HashSet<string> Fields), IEnrichedContentEntity>(nameof(GetContentsLoaderWithFields),
            async (batch, ct) =>
            {
                var fields = batch.SelectMany(x => x.Fields).ToHashSet();

                var result = await QueryContentsByIdsAsync(batch.Select(x => x.Id), fields, ct);

                return result.ToDictionary(x => (x.Id, fields));
            }, maxBatchSize: options.DataLoaderBatchSize);
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
