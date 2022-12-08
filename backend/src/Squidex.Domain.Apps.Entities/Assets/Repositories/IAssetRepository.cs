// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Repositories;

public interface IAssetRepository
{
    IAsyncEnumerable<IAssetEntity> StreamAll(DomainId appId,
        CancellationToken ct = default);

    Task<IResultList<IAssetEntity>> QueryAsync(DomainId appId, DomainId? parentId, Q q,
        CancellationToken ct = default);

    Task<IReadOnlyList<DomainId>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids,
        CancellationToken ct = default);

    Task<IReadOnlyList<DomainId>> QueryChildIdsAsync(DomainId appId, DomainId parentId,
        CancellationToken ct = default);

    Task<IAssetEntity?> FindAssetByHashAsync(DomainId appId, string hash, string fileName, long fileSize,
        CancellationToken ct = default);

    Task<IAssetEntity?> FindAssetBySlugAsync(DomainId appId, string slug,
        CancellationToken ct = default);

    Task<IAssetEntity?> FindAssetAsync(DomainId id,
        CancellationToken ct = default);

    Task<IAssetEntity?> FindAssetAsync(DomainId appId, DomainId id,
        CancellationToken ct = default);
}
