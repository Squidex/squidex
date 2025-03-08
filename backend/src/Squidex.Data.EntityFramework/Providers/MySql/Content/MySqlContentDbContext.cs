// ==========================================================================
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

namespace Squidex.Providers.MySql.Content;

public sealed class MySqlContentDbContext(string prefix, string connectionString, string? versionString, IJsonSerializer jsonSerializer)
    : ContentDbContext(prefix, jsonSerializer)
{
    public override SqlDialect Dialect => MySqlDialect.Instance;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var version =
            !string.IsNullOrWhiteSpace(versionString) ?
            ServerVersion.Parse(versionString) :
            ServerVersion.AutoDetect(connectionString);

        optionsBuilder.SetDefaultWarnings();
        optionsBuilder.UseMySql(connectionString, version, options =>
        {
            options.UseMicrosoftJson(MySqlCommonJsonChangeTrackingOptions.FullHierarchyOptimizedSemantically);
            options.MigrationsHistoryTable($"{prefix}MigrationHistory");
        });

        base.OnConfiguring(optionsBuilder);
    }
}
