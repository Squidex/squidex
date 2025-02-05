// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed partial class EFAssetFolderRepository<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : IAssetFolderRepository where TContext : DbContext
{
    public async Task<IResultList<AssetFolder>> QueryAsync(DomainId appId, DomainId? parentId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAssetFolderRepository/QueryAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var assetFolderEntities =
                await dbContext.Set<EFAssetFolderEntity>()
                    .Where(x => x.IndexedAppId == appId)
                    .WhereIf(x => x.ParentId == parentId!.Value, parentId.HasValue)
                    .ToListAsync(ct);

            return ResultList.Create(assetFolderEntities.Count, assetFolderEntities);
        }
    }

    public async Task<IReadOnlyList<DomainId>> QueryChildIdsAsync(DomainId appId, DomainId? parentId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAssetFolderRepository/QueryChildIdsAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var assetFolderIds =
                await dbContext.Set<EFAssetFolderEntity>()
                    .Where(x => x.IndexedAppId == appId)
                    .WhereIf(x => x.ParentId == parentId!.Value, parentId.HasValue)
                    .Select(x => x.Id)
                    .ToListAsync(ct);

            return assetFolderIds;
        }
    }

    public async Task<AssetFolder?> FindAssetFolderAsync(DomainId appId, DomainId id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFAssetFolderRepository/FindAssetFolderAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var documentId = DomainId.Combine(appId, id);
            var assetFolderEntity =
                await dbContext.Set<EFAssetFolderEntity>()
                    .Where(x => x.DocumentId == documentId)
                    .Where(x => !x.IsDeleted)
                    .FirstOrDefaultAsync(ct);

            return assetFolderEntity;
        }
    }
}
