﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed record ApiStats(
        DateTime Date,
        long TotalCalls, double AverageElapsedMs,
        long TotalBytes)
    {
    }
}
