// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using PhenX.EntityFrameworkCore.BulkInsert.SqlServer;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Queries;

namespace Squidex.Providers.SqlServer.App;

public sealed class SqlServerAppDbContext(DbContextOptions options, IJsonSerializer jsonSerializer)
    : AppDbContext(options, jsonSerializer)
{
    public override SqlDialect Dialect => SqlServerDialect.Instance;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseBulkInsertSqlServer();
        base.OnConfiguring(optionsBuilder);
    }
}
