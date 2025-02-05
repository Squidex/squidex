// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure.States;

public static class EFSnapshotBuilder
{
    public static void UseSnapshot<T>(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn, Action<EntityTypeBuilder<EFState<T>>>? configure = null)
        where T : class
    {
        builder.UseSnapshot<T, EFState<T>>(jsonSerializer, jsonColumn, configure);
    }

    public static void UseSnapshot<T, TEntity>(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn, Action<EntityTypeBuilder<TEntity>>? configure = null)
        where TEntity : EFState<T> where T : class
    {
        builder.Entity<TEntity>(b =>
        {
            b.ToTable(GetTableName<T>());
            b.Property(x => x.DocumentId).AsString();
            b.Property(x => x.Document).AsJsonString(jsonSerializer, jsonColumn);

            configure?.Invoke(b);
        });
    }

    private static string GetTableName<T>()
    {
        var attribute = typeof(T).GetCustomAttributes(true).OfType<CollectionNameAttribute>().FirstOrDefault();

        var tableSuffix = attribute?.Name ?? typeof(T).Name;
        var tableName = $"States_{tableSuffix}";

        return tableName;
    }
}
