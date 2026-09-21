// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Microsoft.EntityFrameworkCore;

public static class EFScriptLogBuilder
{
    public static void UseScriptLogs(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn)
    {
        builder.Entity<ScriptLogRecord>(b =>
        {
            b.ToTable("ScriptLogs");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.AppId, x.Timestamp });
            b.HasIndex(x => x.Timestamp);
            b.Property(x => x.Id).AsString();
            b.Property(x => x.AppId).AsString();
            b.Property(x => x.Name).HasMaxLength(255);
            b.Property(x => x.Entries).AsJsonString(jsonSerializer, jsonColumn);
            b.Property(x => x.Timestamp).AsDateTimeOffset();
        });
    }
}
