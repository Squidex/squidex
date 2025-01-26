// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.States;
using Squidex.Shared;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
#pragma warning disable MA0048 // File name must match type name

namespace Squidex.EntityFramework.TestHelpers;

public class TestDbContextMySql(DbContextOptions options, IJsonSerializer jsonSerializer)
    : TestDbContextBase(options, jsonSerializer)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<TestEntity>()
            .Property(p => p.Json).HasColumnType("json");

        base.OnModelCreating(builder);
    }
}

public class TestDbContext(DbContextOptions options, IJsonSerializer jsonSerializer)
    : TestDbContextBase(options, jsonSerializer)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<TestEntity>()
            .OwnsOne(p => p.Json).ToJson();

        builder.Entity<EFAssetEntity>()
            .Property(p => p.Metadata).HasColumnType("json");

        base.OnModelCreating(builder);
    }
}

public class TestDbContextBase(DbContextOptions options, IJsonSerializer jsonSerializer)
    : AppDbContext(options, jsonSerializer)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.AddSnapshot<SnapshotValue, EFState<SnapshotValue>>(jsonSerializer);

        base.OnModelCreating(builder);
    }
}
