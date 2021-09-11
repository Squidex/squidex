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

namespace Squidex.Domain.Apps.Entities.Assets.Repositories
{
    public interface IAssetFolderRepository
    {
        Task<IResultList<IAssetFolderEntity>> QueryAsync(DomainId appId, DomainId parentId,
            CancellationToken ct = default);

        Task<IReadOnlyList<DomainId>> QueryChildIdsAsync(DomainId appId, DomainId parentId,
            CancellationToken ct = default);

        Task<IAssetFolderEntity?> FindAssetFolderAsync(DomainId appId, DomainId id,
            CancellationToken ct = default);
    }
}
