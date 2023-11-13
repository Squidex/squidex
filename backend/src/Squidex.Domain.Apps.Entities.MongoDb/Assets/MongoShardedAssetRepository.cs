// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets;

public sealed class MongoShardedAssetRepository : ShardedSnapshotStore<MongoAssetRepository, AssetDomainObject.State>, IAssetRepository
{
    public MongoShardedAssetRepository(IShardingStrategy sharding, Func<string, MongoAssetRepository> factory)
        : base(sharding, factory)
    {
    }

    public IEnumerable<IMongoCollection<MongoAssetEntity>> GetInternalCollections()
    {
        return Shards.Select(x => x.GetInternalCollection());
    }

    public async Task<IAssetEntity?> FindAssetAsync(DomainId id,
        CancellationToken ct = default)
    {
        IAssetEntity? result = null;

        foreach (var shard in Shards)
        {
            if ((result = await shard.FindAssetAsync(id, ct)) != null)
            {
                return result;
            }
        }

        return result;
    }

    public Task<IAssetEntity?> FindAssetAsync(DomainId appId, DomainId id, bool allowDeleted,
        CancellationToken ct = default)
    {
        return Shard(appId).FindAssetAsync(appId, id, allowDeleted, ct);
    }

    public Task<IAssetEntity?> FindAssetByHashAsync(DomainId appId, string hash, string fileName, long fileSize,
        CancellationToken ct = default)
    {
        return Shard(appId).FindAssetByHashAsync(appId, hash, fileName, fileSize, ct);
    }

    public Task<IAssetEntity?> FindAssetBySlugAsync(DomainId appId, string slug, bool allowDeleted,
        CancellationToken ct = default)
    {
        return Shard(appId).FindAssetBySlugAsync(appId, slug, allowDeleted, ct);
    }

    public Task<IResultList<IAssetEntity>> QueryAsync(DomainId appId, DomainId? parentId, Q q,
        CancellationToken ct = default)
    {
        return Shard(appId).QueryAsync(appId, parentId, q, ct);
    }

    public Task<IReadOnlyList<DomainId>> QueryChildIdsAsync(DomainId appId, DomainId parentId,
        CancellationToken ct = default)
    {
        return Shard(appId).QueryChildIdsAsync(appId, parentId, ct);
    }

    public Task<IReadOnlyList<DomainId>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids,
        CancellationToken ct = default)
    {
        return Shard(appId).QueryIdsAsync(appId, ids, ct);
    }

    public IAsyncEnumerable<IAssetEntity> StreamAll(DomainId appId,
        CancellationToken ct = default)
    {
        return Shard(appId).StreamAll(appId, ct);
    }

    protected override string GetShardKey(AssetDomainObject.State state)
    {
        return GetShardKey(state.AppId.Id);
    }
}
