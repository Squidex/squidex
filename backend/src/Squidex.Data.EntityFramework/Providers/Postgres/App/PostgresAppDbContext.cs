// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using PhenX.EntityFrameworkCore.BulkInsert.PostgreSql;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Queries;

namespace Squidex.Providers.Postgres.App;

public class PostgresAppDbContext(DbContextOptions options, IJsonSerializer jsonSerializer)
    : AppDbContext(options, jsonSerializer)
{
    public override SqlDialect Dialect => PostgresDialect.Instance;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseBulkInsertPostgreSql();
        base.OnConfiguring(optionsBuilder);
    }
}
