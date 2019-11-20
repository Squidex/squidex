// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.Assets.Repositories
{
    public interface IAssetRepository
    {
        Task<IReadOnlyList<IAssetItemEntity>> QueryByHashAsync(Guid appId, string hash);

        Task<IResultList<IAssetItemEntity>> QueryAsync(Guid appId, Guid? parentId, ClrQuery query);

        Task<IResultList<IAssetItemEntity>> QueryAsync(Guid appId, HashSet<Guid> ids);

        Task<IAssetItemEntity?> FindAssetAsync(Guid id, bool allowDeleted = false);

        Task<IAssetItemEntity?> FindAssetBySlugAsync(Guid appId, string slug);

        Task RemoveAsync(Guid appId);
    }
}
