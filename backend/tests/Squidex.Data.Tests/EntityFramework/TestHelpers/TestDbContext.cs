// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.States;
using Squidex.Shared;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
#pragma warning disable MA0048 // File name must match type name

namespace Squidex.EntityFramework.TestHelpers;

public class TestDbContextMySql(DbContextOptions options, IJsonSerializer jsonSerializer)
    : TestDbContext(options, jsonSerializer)
{
    protected override string? JsonColumnType()
    {
        return "json";
    }
}

public class TestDbContextPostgres(DbContextOptions options, IJsonSerializer jsonSerializer)
    : TestDbContext(options, jsonSerializer)
{
    protected override string? JsonColumnType()
    {
        return "jsonb";
    }
}

public class TestDbContextSqlServer(DbContextOptions options, IJsonSerializer jsonSerializer)
    : TestDbContext(options, jsonSerializer)
{
}

public class TestDbContext(DbContextOptions options, IJsonSerializer jsonSerializer)
    : AppDbContext(options, jsonSerializer)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseSnapshot<SnapshotValue, EFState<SnapshotValue>>(jsonSerializer, JsonColumnType());

        builder.Entity<TestEntity>(b =>
        {
            b.Property(x => x.Json).AsJsonString(jsonSerializer, JsonColumnType());
        });

        base.OnModelCreating(builder);
    }
}
