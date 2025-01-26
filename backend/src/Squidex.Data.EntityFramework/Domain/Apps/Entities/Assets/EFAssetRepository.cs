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
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed partial class EFAssetRepository<TContext>(IDbContextFactory<TContext> dbContextFactory, SqlDialect dialect)
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

            var query = q.Query;
            if (q.Ids is { Count: > 0 })
            {
                var assetEntities =
                    await dbContext.Set<EFAssetEntity>()
                        .Where(x => x.IndexedAppId == appId)
                        .Where(x => q.Ids.Contains(x.Id))
                        .Where(x => !x.IsDeleted)
                        .ToListAsync(ct);
                long assetTotal = assetEntities.Count;

                if (assetEntities.Count >= query.Take || query.Skip > 0)
                {
                    if (q.NoTotal)
                    {
                        assetTotal = -1;
                    }
                    else
                    {
                        assetTotal =
                            await dbContext.Set<EFAssetEntity>()
                                .Where(x => x.IndexedAppId == appId)
                                .Where(x => q.Ids.Contains(x.Id))
                                .Where(x => !x.IsDeleted)
                                .CountAsync(ct);
                    }
                }

                return ResultList.Create(assetTotal, assetEntities.OfType<Asset>());
            }
            else
            {
                var sqlQuery =
                    new AssetSqlQueryBuilder(dialect)
                        .WithLimit(query)
                        .WithOffset(query)
                        .WithOrders(query)
                        .Where(nameof(EFAssetEntity.IndexedAppId), CompareOperator.Equals, appId.ToString());

                if (query.Filter?.HasField("IsDeleted") != true)
                {
                    sqlQuery.Where(nameof(EFAssetEntity.IsDeleted), CompareOperator.Equals, false);
                }

                if (parentId != null)
                {
                    sqlQuery.Where(nameof(EFAssetEntity.ParentId), CompareOperator.Equals, parentId.ToString());
                }

                sqlQuery.WithFilter(query);

                var (sql, parameters) = sqlQuery.Compile();

                var assetEntities = await dbContext.Set<EFAssetEntity>().FromSqlRaw(sql, parameters).ToListAsync(ct);
                var assetTotal = (long)assetEntities.Count;

                if (assetEntities.Count >= query.Take || query.Skip > 0)
                {
                    if (q.NoTotal || q.NoSlowTotal)
                    {
                        assetTotal = -1;
                    }
                    else
                    {
                        sqlQuery
                            .WithCount()
                            .WithoutOrder()
                            .WithLimit(long.MaxValue)
                            .WithOffset(0);

                        var (countSql, countParams) = sqlQuery.Compile();

                        assetTotal =
                            await dbContext.Database.SqlQueryRaw<long>(countSql, countParams, ct)
                                .FirstOrDefaultAsync(ct);
                    }
                }

                if (query.Random > 0)
                {
                    assetEntities = assetEntities.TakeRandom(query.Random).ToList();
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
                    .Where(x => x.IndexedAppId == appId)
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
                    .Where(x => x.IndexedAppId == appId)
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
                    .Where(x => x.Id == id)
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
