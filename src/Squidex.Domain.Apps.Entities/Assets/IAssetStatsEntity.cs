﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public interface IAssetStatsEntity
    {
        DateTime Date { get; }

        long TotalSize { get; }

        long TotalCount { get; }
    }
}
