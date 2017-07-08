// ==========================================================================
//  IAssetDaySizeRepository.cs
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
    public interface IAssetStatsRepository
    {
        Task<IReadOnlyList<IAssetStatsEntity>> QueryAsync(Guid appId, DateTime fromDate, DateTime toDate);

        Task<long> GetTotalSizeAsync(Guid appId);
    }
}
