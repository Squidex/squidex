// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public interface IAssetQueryService
    {
        int DefaultPageSizeGraphQl { get; }

        Task<IReadOnlyList<IAssetEntityEnriched>> QueryByHashAsync(Guid appId, string hash);

        Task<IResultList<IAssetEntityEnriched>> QueryAsync(QueryContext contex, Q query);

        Task<IAssetEntityEnriched> FindAssetAsync(Guid id);
    }
}
