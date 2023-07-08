// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.DataLoader;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.Queries;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public sealed class GraphQLExecutionContext : QueryExecutionContext
{
    private static readonly List<IEnrichedAssetEntity> EmptyAssets = new List<IEnrichedAssetEntity>();
    private static readonly List<IEnrichedContentEntity> EmptyContents = new List<IEnrichedContentEntity>();
    private readonly IDataLoaderContextAccessor dataLoaders;

    public override Context Context { get; }

    public GraphQLExecutionContext(
        IDataLoaderContextAccessor dataLoaders,
        IAssetQueryService assetQuery,
        IAssetCache assetCache,
        IContentQueryService contentQuery,
        IContentCache contentCache,
        IServiceProvider serviceProvider,
        Context context)
        : base(assetQuery, assetCache, contentQuery, contentCache, serviceProvider)
    {
        this.dataLoaders = dataLoaders;

        Context = context.Clone(b => b
            .WithoutCleanup()
            .WithoutContentEnrichment()
            .WithoutAssetEnrichment());
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

    public async Task<IEnrichedAssetEntity?> GetAssetAsync(DomainId id, TimeSpan cacheDuration,
        CancellationToken ct)
    {
        var assets = await GetAssetsAsync(new List<DomainId> { id }, cacheDuration, ct);
        var asset = assets.FirstOrDefault();

        return asset;
    }

    public async Task<IEnrichedContentEntity?> GetContentAsync(DomainId schemaId, DomainId id, HashSet<string>? fields, TimeSpan cacheDuration,
        CancellationToken ct)
    {
        var contents = await GetContentsAsync(new List<DomainId> { id }, fields, cacheDuration, ct);
        var content = contents.FirstOrDefault(x => x.SchemaId.Id == schemaId);

        return content;
    }

    public async Task<IReadOnlyList<IEnrichedAssetEntity>> GetAssetsAsync(List<DomainId>? ids, TimeSpan cacheDuration,
        CancellationToken ct)
    {
        if (ids == null || ids.Count == 0)
        {
            return EmptyAssets;
        }

        async Task<IReadOnlyList<IEnrichedAssetEntity>> LoadAsync(IEnumerable<DomainId> ids)
        {
            var result = await GetAssetsLoader().LoadAsync(ids).GetResultAsync(ct);

            return result?.NotNull().ToList() ?? EmptyAssets;
        }

        if (cacheDuration > TimeSpan.Zero)
        {
            var assets = await AssetCache.CacheOrQueryAsync(ids, async pendingIds =>
            {
                return await LoadAsync(pendingIds);
            }, cacheDuration);

            return assets;
        }

        return await LoadAsync(ids);
    }

    public async Task<IReadOnlyList<IEnrichedContentEntity>> GetContentsAsync(List<DomainId>? ids, HashSet<string>? fields, TimeSpan cacheDuration,
        CancellationToken ct)
    {
        if (ids == null || ids.Count == 0)
        {
            return EmptyContents;
        }

        if (cacheDuration > TimeSpan.Zero || fields == null)
        {
            var contents = await ContentCache.CacheOrQueryAsync(ids, async pendingIds =>
            {
                var result = await GetContentsLoader().LoadAsync(ids).GetResultAsync(ct);

                return result?.NotNull().ToList() ?? EmptyContents;
            }, cacheDuration);

            return contents.ToList();
        }
        else
        {
            var contents = await GetContentsLoaderWithFields().LoadAsync(ids.Select(x => (x, fields))).GetResultAsync(ct);

            return contents?.NotNull().ToList() ?? EmptyContents;
        }
    }

    private IDataLoader<DomainId, IEnrichedAssetEntity> GetAssetsLoader()
    {
        return dataLoaders.Context!.GetOrAddBatchLoader<DomainId, IEnrichedAssetEntity>(nameof(GetAssetsLoader),
            async (batch, ct) =>
            {
                var result = await QueryAssetsByIdsAsync(new List<DomainId>(batch), ct);

                return result.ToDictionary(x => x.Id);
            });
    }

    private IDataLoader<DomainId, IEnrichedContentEntity> GetContentsLoader()
    {
        return dataLoaders.Context!.GetOrAddBatchLoader<DomainId, IEnrichedContentEntity>(nameof(GetContentsLoader),
            async (batch, ct) =>
            {
                var result = await QueryContentsByIdsAsync(batch, null, ct);

                return result.ToDictionary(x => x.Id);
            });
    }

    private IDataLoader<(DomainId Id, HashSet<string> Fields), IEnrichedContentEntity> GetContentsLoaderWithFields()
    {
        return dataLoaders.Context!.GetOrAddBatchLoader<(DomainId Id, HashSet<string> Fields), IEnrichedContentEntity>(nameof(GetContentsLoader),
            async (batch, ct) =>
            {
                var fields = batch.SelectMany(x => x.Fields).ToHashSet();

                var result = await QueryContentsByIdsAsync(batch.Select(x => x.Id), fields, ct);

                return result.ToDictionary(x => (x.Id, fields));
            });
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
}
