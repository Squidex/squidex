// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Microsoft.EntityFrameworkCore;

public static class EFAssetBuilder
{
    public static void UseAssets(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn)
    {
        builder.Entity<EFAssetEntity>(b =>
        {
            b.Property(x => x.Id).AsString();
            b.Property(x => x.AppId).AsString();
            b.Property(x => x.Created).AsDateTimeOffset();
            b.Property(x => x.CreatedBy).AsString();
            b.Property(x => x.DocumentId).AsString();
            b.Property(x => x.IndexedAppId).AsString();
            b.Property(x => x.LastModified).AsDateTimeOffset();
            b.Property(x => x.LastModifiedBy).AsString();
            b.Property(x => x.Metadata).AsJsonString(jsonSerializer, jsonColumn);
            b.Property(x => x.ParentId).AsString();
            b.Property(x => x.Tags).AsString();
            b.Property(x => x.Type).AsString();
        });

        builder.Entity<EFAssetFolderEntity>(b =>
        {
            b.Property(x => x.Id).AsString();
            b.Property(x => x.AppId).AsString();
            b.Property(x => x.Created).AsDateTimeOffset();
            b.Property(x => x.CreatedBy).AsString();
            b.Property(x => x.DocumentId).AsString();
            b.Property(x => x.IndexedAppId).AsString();
            b.Property(x => x.LastModified).AsDateTimeOffset();
            b.Property(x => x.LastModifiedBy).AsString();
            b.Property(x => x.ParentId).AsString();
        });
    }
}
