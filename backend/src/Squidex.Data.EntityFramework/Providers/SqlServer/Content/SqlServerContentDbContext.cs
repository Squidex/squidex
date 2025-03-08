﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Queries;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace Squidex.Providers.SqlServer.Content;

public sealed class SqlServerContentDbContext(string prefix, string connectionString, IJsonSerializer jsonSerializer)
    : ContentDbContext(prefix, jsonSerializer)
{
    public override SqlDialect Dialect => SqlServerDialect.Instance;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.SetDefaultWarnings();
        optionsBuilder.UseSqlServer(connectionString, options =>
        {
            options.MigrationsHistoryTable($"{prefix}MigrationHistory");
        });

        base.OnConfiguring(optionsBuilder);
    }
}
