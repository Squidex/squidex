// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public interface IAssetQueryService
    {
        Task<IResultList<IEnrichedAssetEntity>> QueryAsync(Context context, DomainId? parentId, Q q,
            CancellationToken ct = default);

        Task<IResultList<IAssetFolderEntity>> QueryAssetFoldersAsync(Context context, DomainId parentId,
            CancellationToken ct = default);

        Task<IResultList<IAssetFolderEntity>> QueryAssetFoldersAsync(DomainId appId, DomainId parentId,
            CancellationToken ct = default);

        Task<IReadOnlyList<IAssetFolderEntity>> FindAssetFolderAsync(DomainId appId, DomainId id,
            CancellationToken ct = default);

        Task<IEnrichedAssetEntity?> FindByHashAsync(Context context, string hash, string fileName, long fileSize,
            CancellationToken ct = default);

        Task<IEnrichedAssetEntity?> FindAsync(Context context, DomainId id, long version = EtagVersion.Any,
           CancellationToken ct = default);

        Task<IEnrichedAssetEntity?> FindBySlugAsync(Context context, string slug,
            CancellationToken ct = default);

        Task<IEnrichedAssetEntity?> FindGlobalAsync(Context context, DomainId id,
            CancellationToken ct = default);
    }
}
