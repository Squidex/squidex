// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Repositories
{
    public interface IAssetFolderRepository
    {
        Task<IResultList<IAssetFolderEntity>> QueryAsync(Guid appId, Guid parentId);

        Task<IAssetFolderEntity?> FindAssetFolderAsync(Guid id);
    }
}
