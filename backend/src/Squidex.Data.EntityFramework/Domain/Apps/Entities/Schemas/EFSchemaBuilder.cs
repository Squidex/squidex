// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.States;

namespace Microsoft.EntityFrameworkCore;

public static class EFSchemaBuilder
{
    public static void UseSchema(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn)
    {
        builder.UseSnapshot<Schema, EFSchemaEntity>(jsonSerializer, jsonColumn, b =>
        {
            b.Property(x => x.IndexedAppId).AsString();
            b.Property(x => x.IndexedId).AsString();
        });
    }
}
