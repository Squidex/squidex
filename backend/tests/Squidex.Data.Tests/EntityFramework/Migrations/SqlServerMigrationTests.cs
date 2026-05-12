// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Queries;
using Squidex.Providers.SqlServer;
using Squidex.Providers.SqlServer.App;
using Testcontainers.MsSql;

namespace Squidex.EntityFramework.Migrations;

[Trait("Category", "TestContainer")]
public class SqlServerMigrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer sqlServer =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
            .Build();

    public async ValueTask InitializeAsync()
    {
        await sqlServer.StartAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await sqlServer.DisposeAsync();
    }

    [Fact]
    public async Task Should_migrate()
    {
        var services =
            new ServiceCollection()
                .AddDbContextFactory<SqlServerAppDbContext>(b =>
                {
                    b.UseSqlServer(sqlServer.GetConnectionString(), options =>
                    {
                        options.UseNetTopologySuite();
                    });
                })
                .AddSingleton<ConnectionStringParser, SqlServerConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddSingleton<DatabaseMigrator<SqlServerAppDbContext>>()
                .BuildServiceProvider();

        var databaseMigrator = services.GetRequiredService<DatabaseMigrator<SqlServerAppDbContext>>();
        var databaseFactory = services.GetRequiredService<IDbContextFactory<SqlServerAppDbContext>>();

        await databaseMigrator.InitializeAsync(default);

        await using var dbContext = await databaseFactory.CreateDbContextAsync();

        var migrations = await dbContext.Database.GetAppliedMigrationsAsync();
        Assert.NotEmpty(migrations);
    }

    [Fact]
    public async Task Should_migrate_idempotent_and_functions_callable()
    {
        var services =
            new ServiceCollection()
                .AddDbContextFactory<SqlServerAppDbContext>(b =>
                {
                    b.UseSqlServer(sqlServer.GetConnectionString(), options =>
                    {
                        options.UseNetTopologySuite();
                    });
                })
                .AddSingleton<ConnectionStringParser, SqlServerConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddSingleton<DatabaseMigrator<SqlServerAppDbContext>>()
                .BuildServiceProvider();

        var databaseMigrator = services.GetRequiredService<DatabaseMigrator<SqlServerAppDbContext>>();
        var databaseFactory = services.GetRequiredService<IDbContextFactory<SqlServerAppDbContext>>();

        // Run migrations twice to verify idempotency.
        await databaseMigrator.InitializeAsync(default);
        await databaseMigrator.InitializeAsync(default);

        // Verify the dbo.json_exists function was created and is callable.
        // Note: {{ and }} are escaped braces for ExecuteSqlRawAsync's string.Format-style parser.
        await using var dbContext = await databaseFactory.CreateDbContextAsync();
        var result = await dbContext.Database.ExecuteSqlRawAsync(
            "SELECT dbo.json_exists('{{\"a\":1}}', '$.a')");
        Assert.Equal(-1, result);
    }

    [Fact]
    public async Task Should_migrate_when_functions_already_exist()
    {
        var services =
            new ServiceCollection()
                .AddDbContextFactory<SqlServerAppDbContext>(b =>
                {
                    b.UseSqlServer(sqlServer.GetConnectionString(), options =>
                    {
                        options.UseNetTopologySuite();
                    });
                })
                .AddSingleton<ConnectionStringParser, SqlServerConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddSingleton<DatabaseMigrator<SqlServerAppDbContext>>()
                .BuildServiceProvider();

        var databaseMigrator = services.GetRequiredService<DatabaseMigrator<SqlServerAppDbContext>>();
        var databaseFactory = services.GetRequiredService<IDbContextFactory<SqlServerAppDbContext>>();

        // Simulate a pre-migration database: apply all migrations up to (but not including)
        // AddJsonFunctions, then create the functions via the old SqlDialectInitializer path.
        await using (var dbContext = await databaseFactory.CreateDbContextAsync())
        {
            await dbContext.Database.MigrateAsync("20260323155035_MigrateToNet10");

            if (dbContext is IDbContextWithDialect withDialect)
            {
                await withDialect.Dialect.InitializeAsync(dbContext, default);
            }
        }

        // Run the full migration — should apply AddJsonFunctions on top of already-existing functions.
        await databaseMigrator.InitializeAsync(default);

        // Verify the functions are still callable after migration.
        // Note: {{ and }} are escaped braces for ExecuteSqlRawAsync's string.Format-style parser.
        await using var verifyContext = await databaseFactory.CreateDbContextAsync();
        var result = await verifyContext.Database.ExecuteSqlRawAsync(
            "SELECT dbo.json_exists('{{\"a\":1}}', '$.a')");
        Assert.Equal(-1, result);
    }
}
