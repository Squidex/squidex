// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets;

public interface IAssetQueryService
{
    Task<IResultList<EnrichedAsset>> QueryAsync(Context context, DomainId? parentId, Q q,
        CancellationToken ct = default);

    Task<IResultList<AssetFolder>> QueryAssetFoldersAsync(Context context, DomainId? parentId,
        CancellationToken ct = default);

    Task<IReadOnlyList<AssetFolder>> FindAssetFolderAsync(DomainId appId, DomainId id,
        CancellationToken ct = default);

    Task<EnrichedAsset?> FindByHashAsync(Context context, string hash, string fileName, long fileSize,
        CancellationToken ct = default);

    Task<EnrichedAsset?> FindAsync(Context context, DomainId id, bool allowDeleted = false, long version = EtagVersion.Any,
       CancellationToken ct = default);

    Task<EnrichedAsset?> FindBySlugAsync(Context context, string slug, bool allowDeleted = false,
        CancellationToken ct = default);

    Task<EnrichedAsset?> FindGlobalAsync(Context context, DomainId id,
        CancellationToken ct = default);
}
