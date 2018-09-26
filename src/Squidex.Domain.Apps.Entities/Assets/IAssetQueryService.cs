// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public interface IAssetQueryService
    {
        Task<IResultList<IAssetEntity>> QueryAsync(QueryContext contex, Q query);

        Task<IAssetEntity> FindAssetAsync(QueryContext context, Guid id);
    }
}
