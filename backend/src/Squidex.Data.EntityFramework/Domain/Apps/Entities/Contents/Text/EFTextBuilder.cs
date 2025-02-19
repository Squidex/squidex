// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Infrastructure;

namespace Microsoft.EntityFrameworkCore;

public static class EFTextBuilder
{
    public static void UseTextIndex(this ModelBuilder builder)
    {
        builder.Entity<TextContentState>(b =>
        {
            b.ToTable("TextState");
            b.HasKey(x => x.UniqueContentId);
            b.Property(x => x.UniqueContentId).AsString();
            b.Property(x => x.State).AsString();
        });

        builder.Entity<EFTextIndexEntity>(b =>
        {
            b.ToTable("Texts");
            b.Property(x => x.Id).HasMaxLength(400);
            b.Property(x => x.AppId).AsString();
            b.Property(x => x.SchemaId).AsString();
            b.Property(x => x.ContentId).AsString();
        });

        builder.Entity<EFGeoEntity>(b =>
        {
            b.ToTable("Geos");
            b.Property(x => x.Id).HasMaxLength(400);
            b.Property(x => x.AppId).AsString();
            b.Property(x => x.SchemaId).AsString();
            b.Property(x => x.ContentId).AsString();
        });
    }
}
