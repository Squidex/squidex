// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Microsoft.EntityFrameworkCore;

public static class EFContentBuilder
{
    public static void UseContent(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn, string prefix)
    {
        builder.UseContentEntity<EFContentCompleteEntity>(jsonSerializer, jsonColumn, $"{prefix}ContentsAll");
        builder.UseContentEntity<EFContentPublishedEntity>(jsonSerializer, jsonColumn, $"{prefix}ContentsPublished");        
    }

    public static void UseContentReferences(this ModelBuilder builder, string prefix)
    {
        builder.UseContentReference<EFReferenceCompleteEntity>($"{prefix}ContentReferencesAll");
        builder.UseContentReference<EFReferencePublishedEntity>($"{prefix}ContentReferencesPublished");
    }

    public static void UseContentTables(this ModelBuilder builder)
    {
        builder.Entity<EFContentTableEntity>(b =>
        {
            b.ToTable("ContentTables");
            b.HasIndex("AppId", "SchemaId").IsUnique();

            b.Property(x => x.AppId).AsString();
            b.Property(x => x.SchemaId).AsString();
        });
    }

    private static void UseContentEntity<T>(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn, string tableName)
        where T : EFContentEntity
    {
        builder.Entity<T>(b =>
        {
            b.ToTable(tableName);
            b.Property(x => x.Id).AsString();
            b.Property(x => x.AppId).AsString();
            b.Property(x => x.Created).AsDateTimeOffset();
            b.Property(x => x.CreatedBy).AsString();
            b.Property(x => x.Data).AsJsonString(jsonSerializer, jsonColumn);
            b.Property(x => x.DocumentId).AsString();
            b.Property(x => x.IndexedAppId).AsString();
            b.Property(x => x.IndexedSchemaId).AsString();
            b.Property(x => x.LastModified).AsDateTimeOffset();
            b.Property(x => x.LastModifiedBy).AsString();
            b.Property(x => x.NewData).AsNullableJsonString(jsonSerializer, jsonColumn);
            b.Property(x => x.NewStatus).AsNullableString();
            b.Property(x => x.SchemaId).AsString();
            b.Property(x => x.ScheduledAt).AsDateTimeOffset();
            b.Property(x => x.ScheduleJob).AsNullableJsonString(jsonSerializer, jsonColumn);
            b.Property(x => x.Status).AsString();
            b.Property(x => x.TranslationStatus).AsNullableJsonString(jsonSerializer, jsonColumn);
        });
    }

    private static void UseContentReference<T>(this ModelBuilder builder, string tableName)
        where T : EFContentReferenceEntity
    {
        builder.Entity<T>(b =>
        {
            b.ToTable(tableName);
            b.HasKey("AppId", "FromKey", "ToId");

            b.Property(x => x.AppId).AsString();
            b.Property(x => x.FromKey).AsString();
            b.Property(x => x.FromSchema).AsString().HasDefaultValue(DomainId.Empty);
            b.Property(x => x.ToId).AsString();
        });
    }
}
