// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Repositories
{
    public interface IAssetRepository
    {
        IAsyncEnumerable<IAssetEntity> StreamAll(DomainId appId);

        Task<IResultList<IAssetEntity>> QueryAsync(DomainId appId, DomainId? parentId, Q q);

        Task<IReadOnlyList<DomainId>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids);

        Task<IReadOnlyList<DomainId>> QueryChildIdsAsync(DomainId appId, DomainId parentId);

        Task<IAssetEntity?> FindAssetByHashAsync(DomainId appId, string hash, string fileName, long fileSize);

        Task<IAssetEntity?> FindAssetBySlugAsync(DomainId appId, string slug);

        Task<IAssetEntity?> FindAssetAsync(DomainId appId);

        Task<IAssetEntity?> FindAssetAsync(DomainId appId, DomainId id);
    }
}
