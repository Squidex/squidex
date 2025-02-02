// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.History;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Microsoft.EntityFrameworkCore;

public static class EFHistoryBuilder
{
    public static void UseHistory(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn)
    {
        builder.Entity<HistoryEvent>(b =>
        {
            b.Property(x => x.Actor).AsString();
            b.Property(x => x.Id).AsString();
            b.Property(x => x.OwnerId).AsString();
            b.Property(x => x.Parameters).AsJsonString(jsonSerializer, jsonColumn);
            b.Property(x => x.Created).AsDateTimeOffset();
        });
    }
}
