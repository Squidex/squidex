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
using Squidex.Infrastructure.Json.Objects;
using Squidex.Shared.Users;

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
            .WithoutContentEnrichment());
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

    public async Task<IEnrichedAssetEntity?> FindAssetAsync(DomainId id,
        CancellationToken ct)
    {
        var dataLoader = GetAssetsLoader();

        return await dataLoader.LoadAsync(id).GetResultAsync(ct);
    }

    public async Task<IContentEntity?> FindContentAsync(DomainId schemaId, DomainId id,
        CancellationToken ct)
    {
        var dataLoader = GetContentsLoader();

        var content = await dataLoader.LoadAsync(id).GetResultAsync(ct);

        if (content?.SchemaId.Id != schemaId)
        {
            content = null;
        }

        return content;
    }

    public Task<IReadOnlyList<IEnrichedAssetEntity>> GetReferencedAssetsAsync(JsonValue value, TimeSpan cacheDuration,
        CancellationToken ct)
    {
        var ids = ParseIds(value);

        return GetAssetsAsync(ids, cacheDuration, ct);
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

    public Task<IReadOnlyList<IEnrichedContentEntity>> GetReferencedContentsAsync(JsonValue value, TimeSpan cacheDuration,
        CancellationToken ct)
    {
        var ids = ParseIds(value);

        return GetContentsAsync(ids, cacheDuration, ct);
    }

    public async Task<IReadOnlyList<IEnrichedContentEntity>> GetContentsAsync(List<DomainId>? ids, TimeSpan cacheDuration,
        CancellationToken ct)
    {
        if (ids == null || ids.Count == 0)
        {
            return EmptyContents;
        }

        async Task<IReadOnlyList<IEnrichedContentEntity>> LoadAsync(IEnumerable<DomainId> ids)
        {
            var result = await GetContentsLoader().LoadAsync(ids).GetResultAsync(ct);

            return result?.NotNull().ToList() ?? EmptyContents;
        }

        if (cacheDuration > TimeSpan.Zero)
        {
            var contents = await ContentCache.CacheOrQueryAsync(ids, async pendingIds =>
            {
                return await LoadAsync(pendingIds);
            }, cacheDuration);

            return contents.ToList();
        }

        return await LoadAsync(ids);
    }

    private IDataLoader<DomainId, IEnrichedAssetEntity> GetAssetsLoader()
    {
        return dataLoaders.Context!.GetOrAddBatchLoader<DomainId, IEnrichedAssetEntity>(nameof(GetAssetsLoader),
            async (batch, ct) =>
            {
                var result = await GetReferencedAssetsAsync(new List<DomainId>(batch), ct);

                return result.ToDictionary(x => x.Id);
            });
    }

    private IDataLoader<DomainId, IEnrichedContentEntity> GetContentsLoader()
    {
        return dataLoaders.Context!.GetOrAddBatchLoader<DomainId, IEnrichedContentEntity>(nameof(GetContentsLoader),
            async (batch, ct) =>
            {
                var result = await GetReferencedContentsAsync(new List<DomainId>(batch), ct);

                return result.ToDictionary(x => x.Id);
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

    private static List<DomainId>? ParseIds(JsonValue value)
    {
        try
        {
            List<DomainId>? result = null;

            if (value.Value is JsonArray a)
            {
                foreach (var item in a)
                {
                    if (item.Value is string id)
                    {
                        result ??= new List<DomainId>();
                        result.Add(DomainId.Create(id));
                    }
                }
            }

            return result;
        }
        catch
        {
            return null;
        }
    }
}
