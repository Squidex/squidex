// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Repositories;

public interface IAssetFolderRepository
{
    Task<IResultList<AssetFolder>> QueryAsync(DomainId appId, DomainId? parentId,
        CancellationToken ct = default);

    Task<IReadOnlyList<DomainId>> QueryChildIdsAsync(DomainId appId, DomainId? parentId,
        CancellationToken ct = default);

    Task<AssetFolder?> FindAssetFolderAsync(DomainId appId, DomainId id,
        CancellationToken ct = default);
}
