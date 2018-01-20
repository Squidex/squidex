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

namespace Squidex.Domain.Apps.Entities.Assets.Repositories
{
    public interface IAssetRepository
    {
        Task<IResultList<IAssetEntity>> QueryAsync(Guid appId, HashSet<string> mimeTypes = null, HashSet<Guid> ids = null, string query = null, int take = 10, int skip = 0);

        Task<IAssetEntity> FindAssetAsync(Guid id);
    }
}
