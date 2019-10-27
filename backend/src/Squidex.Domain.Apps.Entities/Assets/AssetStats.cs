// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetStats
    {
        public DateTime Date { get; }

        public long TotalCount { get; }

        public long TotalSize { get; }

        public AssetStats(DateTime date, long totalCount, long totalSize)
        {
            Date = date;

            TotalCount = totalCount;
            TotalSize = totalSize;
        }
    }
}
