﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed record ApiStatsSummary(
        double AverageElapsedMs,
        long TotalCalls,
        long TotalBytes,
        long MonthCalls,
        long MonthBytes)
    {
    }
}
