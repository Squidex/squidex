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

namespace Squidex.Domain.Apps.Read.Assets.Repositories
{
    public interface IAssetRepository
    {
        Task<IReadOnlyList<IAssetEntity>> QueryAsync(Guid appId, HashSet<string> mimeTypes = null, HashSet<Guid> ids = null, string query = null, int take = 10, int skip = 0);

        Task<IReadOnlyList<Guid>> QueryNotFoundAsync(Guid appId, IList<Guid> assetIds);

        Task<IAssetEntity> FindAssetAsync(Guid id);

        Task<long> CountAsync(Guid appId, HashSet<string> mimeTypes = null, HashSet<Guid> ids = null, string query = null);
    }
}
