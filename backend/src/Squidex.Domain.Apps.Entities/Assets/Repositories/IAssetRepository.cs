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
        Task<IReadOnlyList<IAssetEntity>> QueryByHashAsync(Guid appId, string hash);

        Task<IResultList<IAssetEntity>> QueryAsync(Guid appId, Guid? parentId, ClrQuery query);

        Task<IResultList<IAssetEntity>> QueryAsync(Guid appId, HashSet<Guid> ids);

        Task<IAssetEntity?> FindAssetAsync(Guid id);

        Task<IAssetEntity?> FindAssetBySlugAsync(Guid appId, string slug);
    }
}
