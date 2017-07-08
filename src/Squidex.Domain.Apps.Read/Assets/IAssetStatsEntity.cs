// ==========================================================================
//  IAssetDaySizeEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Read.Assets
{
    public interface IAssetStatsEntity
    {
        DateTime Date { get; }

        long TotalSize { get; }

        long TotalCount { get; }
    }
}
