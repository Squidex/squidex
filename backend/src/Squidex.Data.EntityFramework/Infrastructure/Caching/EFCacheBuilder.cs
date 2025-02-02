// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Caching;

namespace Microsoft.EntityFrameworkCore;

public static class EFCacheBuilder
{
    public static void UseCache(this ModelBuilder builder)
    {
        builder.Entity<EFCacheEntity>(b =>
        {
            b.ToTable("Cache");
        });
    }
}
