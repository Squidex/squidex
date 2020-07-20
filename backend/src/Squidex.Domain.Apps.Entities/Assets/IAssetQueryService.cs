// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public interface IAssetQueryService
    {
        Task<IReadOnlyList<IEnrichedAssetEntity>> QueryByHashAsync(Context context, DomainId appId, string hash);

        Task<IResultList<IEnrichedAssetEntity>> QueryAsync(Context context, DomainId? parentId, Q query);

        Task<IResultList<IAssetFolderEntity>> QueryAssetFoldersAsync(Context context, DomainId parentId);

        Task<IReadOnlyList<IAssetFolderEntity>> FindAssetFolderAsync(DomainId appId, DomainId id);

        Task<IEnrichedAssetEntity?> FindAssetAsync(Context context, DomainId id);
    }
}
