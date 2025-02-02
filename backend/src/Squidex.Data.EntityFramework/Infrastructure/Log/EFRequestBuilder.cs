// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;

namespace Microsoft.EntityFrameworkCore;

public static class EFRequestBuilder
{
    public static void UseRequest(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn)
    {
        builder.Entity<EFRequestEntity>(b =>
        {
            b.ToTable("Requests");
            b.Property(x => x.Timestamp).AsDateTimeOffset();
            b.Property(x => x.Properties).AsJsonString(jsonSerializer, jsonColumn);
        });
    }
}
