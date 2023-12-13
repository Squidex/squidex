// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Repositories;

public interface IAssetRepository
{
    IAsyncEnumerable<Asset> StreamAll(DomainId appId,
        CancellationToken ct = default);

    Task<IResultList<Asset>> QueryAsync(DomainId appId, DomainId? parentId, Q q,
        CancellationToken ct = default);

    Task<IReadOnlyList<DomainId>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids,
        CancellationToken ct = default);

    Task<IReadOnlyList<DomainId>> QueryChildIdsAsync(DomainId appId, DomainId parentId,
        CancellationToken ct = default);

    Task<Asset?> FindAssetByHashAsync(DomainId appId, string hash, string fileName, long fileSize,
        CancellationToken ct = default);

    Task<Asset?> FindAssetBySlugAsync(DomainId appId, string slug, bool allowDeleted,
        CancellationToken ct = default);

    Task<Asset?> FindAssetAsync(DomainId id,
        CancellationToken ct = default);

    Task<Asset?> FindAssetAsync(DomainId appId, DomainId id, bool allowDeleted,
        CancellationToken ct = default);
}
