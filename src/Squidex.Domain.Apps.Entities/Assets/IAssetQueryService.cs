﻿// ==========================================================================
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
        Task<IReadOnlyList<IEnrichedAssetEntity>> QueryByHashAsync(Guid appId, string hash);

        Task<IResultList<IEnrichedAssetEntity>> QueryAsync(Context context, Q query);

        Task<IEnrichedAssetEntity?> FindAssetAsync(Guid id);
    }
}
