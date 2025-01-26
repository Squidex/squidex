// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets;

public class EFAssetRepository<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : IAssetRepository where TContext : DbContext
{
    public async IAsyncEnumerable<Asset> StreamAll(DomainId appId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var entities =
            dbContext.Set<EFAssetEntity>()
                .Where(x => x.IndexedAppId == appId)
                .Where(x => !x.IsDeleted)
                .ToAsyncEnumerable();

        await foreach (var entity in entities.WithCancellation(ct))
        {
            yield return entity;
        }
    }

    public async Task<IResultList<Asset>> QueryAsync(DomainId appId, DomainId? parentId, Q q,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAssetRepository/QueryAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            if (q.Ids is { Count: > 0 })
            {
                var assetEntities =
                    await dbContext.Set<EFAssetEntity>()
                        .Where(x => x.DocumentId == appId)
                        .Where(x => q.Ids.Contains(x.Id))
                        .Where(x => !x.IsDeleted)
                        .ToListAsync(ct);
                long assetTotal = assetEntities.Count;

                if (assetEntities.Count >= q.Query.Take || q.Query.Skip > 0)
                {
                    if (q.NoTotal)
                    {
                        assetTotal = -1;
                    }
                    else
                    {
                        assetTotal =
                            await dbContext.Set<EFAssetEntity>()
                                .Where(x => x.DocumentId == appId)
                                .Where(x => q.Ids.Contains(x.Id))
                                .Where(x => !x.IsDeleted)
                                .CountAsync(ct);
                    }
                }

                return ResultList.Create(assetTotal, assetEntities.OfType<Asset>());
            }
            else
            {
                // Default means that no other filters are applied and we only query by app.
                var (filter, isDefault) = query.BuildFilter(appId, parentId);

                var assetEntities =
                    await Collection.Find(filter)
                        .QueryLimit(query)
                        .QuerySkip(query)
                        .QuerySort(query)
                        .ToListRandomAsync(Collection, query.Random, ct);
                long assetTotal = assetEntities.Count;

                if (assetEntities.Count >= query.Take || query.Skip > 0)
                {
                    var isDefaultQuery = query.Filter == null;

                    if (q.NoTotal || (q.NoSlowTotal && !isDefaultQuery))
                    {
                        assetTotal = -1;
                    }
                    else if (isDefaultQuery)
                    {
                        // Cache total count by app and asset folder because no other filters are applied (aka default).
                        var totalKey = $"{appId}_{parentId}";

                        assetTotal = await countCollection.GetOrAddAsync(totalKey, ct => Collection.Find(filter).CountDocumentsAsync(ct), ct);
                    }
                    else
                    {
                        assetTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
                    }
                }

                return ResultList.Create<Asset>(assetTotal, assetEntities);
            }
        }
    }

    public async Task<IReadOnlyList<DomainId>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAssetRepository/QueryIdsAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var assetIds =
                await dbContext.Set<EFAssetEntity>()
                    .Where(x => x.DocumentId == appId)
                    .Where(x => ids.Contains(x.Id))
                    .Where(x => !x.IsDeleted)
                    .Select(x => x.Id)
                    .ToListAsync(ct);

            return assetIds;
        }
    }

    public async Task<IReadOnlyList<DomainId>> QueryChildIdsAsync(DomainId appId, DomainId parentId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAssetRepository/QueryChildIdsAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var assetIds =
                await dbContext.Set<EFAssetEntity>()
                    .Where(x => x.DocumentId == appId)
                    .Where(x => x.ParentId == parentId)
                    .Where(x => !x.IsDeleted)
                    .Select(x => x.Id)
                    .ToListAsync(ct);

            return assetIds;
        }
    }

    public async Task<Asset?> FindAssetByHashAsync(DomainId appId, string hash, string fileName, long fileSize,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAssetRepository/FindAssetByHashAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var assetEntity =
                await dbContext.Set<EFAssetEntity>()
                    .Where(x => x.IndexedAppId == appId)
                    .Where(x => x.FileHash == hash && x.FileName == fileName && x.FileSize == fileSize)
                    .Where(x => !x.IsDeleted)
                    .FirstOrDefaultAsync(ct);

            return assetEntity;
        }
    }

    public async Task<Asset?> FindAssetBySlugAsync(DomainId appId, string slug, bool allowDeleted,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAssetRepository/FindAssetBySlugAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var query = dbContext.Set<EFAssetEntity>().Where(x => x.IndexedAppId == appId && x.Slug == slug);
            if (!allowDeleted)
            {
                query = query.Where(x => !x.IsDeleted);
            }

            var assetEntity = await query.FirstOrDefaultAsync(ct);

            return assetEntity;
        }
    }

    public async Task<Asset?> FindAssetAsync(DomainId appId, DomainId id, bool allowDeleted,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAssetRepository/FindAssetAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var query = dbContext.Set<EFAssetEntity>().Where(x => x.IndexedAppId == appId && x.Id == id);
            if (!allowDeleted)
            {
                query = query.Where(x => !x.IsDeleted);
            }

            var assetEntity = await query.FirstOrDefaultAsync(ct);

            return assetEntity;
        }
    }

    public async Task<Asset?> FindAssetAsync(DomainId id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAssetRepository/FindAssetAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var assetEntity =
                await dbContext.Set<EFAssetEntity>()
                    .Where(x => x.DocumentId == id)
                    .Where(x => !x.IsDeleted)
                    .FirstOrDefaultAsync(ct);

            return assetEntity;
        }
    }

    private Task<TContext> CreateDbContextAsync(CancellationToken ct)
    {
        return dbContextFactory.CreateDbContextAsync(ct);
    }
}
