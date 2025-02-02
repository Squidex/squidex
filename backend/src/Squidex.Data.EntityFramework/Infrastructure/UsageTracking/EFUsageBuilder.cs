// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.UsageTracking;

namespace Microsoft.EntityFrameworkCore;

public static class EFUsageBuilder
{
    public static void UseUsage(this ModelBuilder builder)
    {
        builder.Entity<EFUsageCounterEntity>(b =>
        {
            b.ToTable("Counter");
        });
    }
}
