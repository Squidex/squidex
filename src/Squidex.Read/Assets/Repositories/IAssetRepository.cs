// ==========================================================================
//  IAssetRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Read.Assets.Repositories
{
    public interface IAssetRepository
    {
        Task<IReadOnlyList<IAssetEntity>> QueryAsync(Guid appId, HashSet<string> mimeTypes = null, HashSet<Guid> ids = null, string query = null, int take = 10, int skip = 0);

        Task<long> CountAsync(Guid appId, HashSet<string> mimeTypes = null, HashSet<Guid> ids = null, string query = null);

        Task<bool> ExistsAsync(Guid appId, Guid assetId);

        Task<IAssetEntity> FindAssetAsync(Guid id);
    }
}
