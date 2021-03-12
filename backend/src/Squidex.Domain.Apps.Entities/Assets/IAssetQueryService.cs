﻿// ==========================================================================
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
        Task<IResultList<IEnrichedAssetEntity>> QueryAsync(Context context, DomainId? parentId, Q q);

        Task<IResultList<IAssetFolderEntity>> QueryAssetFoldersAsync(Context context, DomainId parentId);

        Task<IReadOnlyList<IAssetFolderEntity>> FindAssetFolderAsync(DomainId appId, DomainId id);

        Task<IEnrichedAssetEntity?> FindByHashAsync(Context context, string hash, string fileName, long fileSize);

        Task<IEnrichedAssetEntity?> FindAsync(Context context, DomainId id, long version = EtagVersion.Any);

        Task<IEnrichedAssetEntity?> FindBySlugAsync(Context context, string slug);

        Task<IEnrichedAssetEntity?> FindGlobalAsync(Context context, DomainId id);
    }
}
