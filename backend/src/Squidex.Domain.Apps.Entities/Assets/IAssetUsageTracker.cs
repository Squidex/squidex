// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public interface IAssetUsageTracker
    {
        Task<IReadOnlyList<AssetStats>> QueryAsync(Guid appId, DateTime fromDate, DateTime toDate);

        Task<long> GetTotalSizeAsync(Guid appId);
    }
}
