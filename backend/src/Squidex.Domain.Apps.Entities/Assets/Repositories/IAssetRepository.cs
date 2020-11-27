// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.Assets.Repositories
{
    public interface IAssetRepository
    {
        IAsyncEnumerable<IAssetEntity> StreamAll(DomainId appId);

        Task<IResultList<IAssetEntity>> QueryAsync(DomainId appId, DomainId? parentId, ClrQuery query);

        Task<IResultList<IAssetEntity>> QueryAsync(DomainId appId, HashSet<DomainId> ids);

        Task<IReadOnlyList<DomainId>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids);

        Task<IReadOnlyList<DomainId>> QueryChildIdsAsync(DomainId appId, DomainId parentId);

        Task<IAssetEntity?> FindAssetAsync(DomainId appId, string hash, string fileName, long fileSize);

        Task<IAssetEntity?> FindAssetAsync(DomainId appId);

        Task<IAssetEntity?> FindAssetAsync(DomainId appId, DomainId id);

        Task<IAssetEntity?> FindAssetBySlugAsync(DomainId appId, string slug);
    }
}
