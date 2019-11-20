// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public interface IAssetQueryService
    {
        Task<IReadOnlyList<IEnrichedAssetItemEntity>> QueryByHashAsync(Context context, Guid appId, string hash);

        Task<IResultList<IEnrichedAssetItemEntity>> QueryAsync(Context context, Guid? parentId, Q query);

        Task<IEnrichedAssetItemEntity?> FindAssetAsync(Context context, Guid id);

        Task<IEnrichedAssetItemEntity?> FindFolderAsync(Context context, Guid id);
    }
}
