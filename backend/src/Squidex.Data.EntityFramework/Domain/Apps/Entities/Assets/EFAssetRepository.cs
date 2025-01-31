// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

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

            if (q.Ids is { Count: > 0 })
            {
                var result =
                    await dbContext.Set<EFAssetEntity>()
                        .Where(x => x.IndexedAppId == appId)
                        .Where(x => q.Ids.Contains(x.Id))
                        .Where(x => !x.IsDeleted)
                        .QueryAsync(q, ct);

                return result;
            }

            var sqlQuery =
                new AssetSqlQueryBuilder(dialect)
                    .Where(ClrFilter.Eq(nameof(EFAssetEntity.IndexedAppId), appId))
                    .Order(q.Query);

            if (q.Query.Filter?.HasField("IsDeleted") != true)
            {
                sqlQuery.Where(ClrFilter.Eq(nameof(EFAssetEntity.IsDeleted), false));
            }

            if (parentId != null)
            {
                sqlQuery.Where(ClrFilter.Eq(nameof(EFAssetEntity.ParentId), parentId));
            }

            sqlQuery.Where(q.Query);

            return await dbContext.QueryAsync<EFAssetEntity>(sqlQuery, q, ct);
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

            var assetEntity =
                await dbContext.Set<EFAssetEntity>()
                    .Where(x => x.IndexedAppId == appId && x.Slug == slug)
                    .WhereIf(x => !x.IsDeleted, !allowDeleted)
                    .FirstOrDefaultAsync(ct);

            return assetEntity;
        }
    }

    public async Task<Asset?> FindAssetAsync(DomainId appId, DomainId id, bool allowDeleted,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAssetRepository/FindAssetAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var docId = DomainId.Combine(appId, id);

            var assetEntity =
                await dbContext.Set<EFAssetEntity>()
                    .Where(x => x.DocumentId == docId)
                    .WhereIf(x => !x.IsDeleted, !allowDeleted)
                    .FirstOrDefaultAsync(ct);

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
