﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public interface IAssetUsageTracker
    {
        Task<IReadOnlyList<AssetStats>> QueryAsync(DomainId appId, DateTime fromDate, DateTime toDate);

        Task<long> GetTotalSizeAsync(DomainId appId);
    }
}
