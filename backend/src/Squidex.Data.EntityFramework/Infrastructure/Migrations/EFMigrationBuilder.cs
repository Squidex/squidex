// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Migrations;

namespace Microsoft.EntityFrameworkCore;

public static class EFMigrationBuilder
{
    public static void UseMigration(this ModelBuilder builder)
    {
        builder.Entity<EFMigrationEntity>(b =>
        {
            b.ToTable("Migrations");
        });
    }
}
