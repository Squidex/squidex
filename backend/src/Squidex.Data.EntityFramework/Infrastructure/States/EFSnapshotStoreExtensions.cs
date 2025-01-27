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

public static class EFSnapshotStoreExtensions
{
    public static ModelBuilder AddSnapshot<T>(this ModelBuilder modelBuilder, IJsonSerializer jsonSerializer, Action<EntityTypeBuilder<EFState<T>>>? builder = null)
        where T : class
    {
        return modelBuilder.AddSnapshot<T, EFState<T>>(jsonSerializer, builder);
    }

    public static ModelBuilder AddSnapshot<T, TEntity>(this ModelBuilder modelBuilder, IJsonSerializer jsonSerializer, Action<EntityTypeBuilder<TEntity>>? builder = null)
        where TEntity : EFState<T> where T : class
    {
        var attribute = typeof(T).GetCustomAttributes(true).OfType<CollectionNameAttribute>().FirstOrDefault();

        var tableSuffix = attribute?.Name ?? typeof(T).Name;
        var tableName = $"States_{tableSuffix}";

        modelBuilder.Entity<TEntity>(b =>
        {
            b.ToTable(tableName);
            b.Property(x => x.DocumentId).AsString();
            b.Property(x => x.Document).AsJsonString(jsonSerializer);

            builder?.Invoke(b);
        });

        return modelBuilder;
    }
}
