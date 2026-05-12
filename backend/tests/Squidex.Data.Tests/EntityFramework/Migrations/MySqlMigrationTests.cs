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
using Squidex.Providers.MySql;
using Squidex.Providers.MySql.App;
using Testcontainers.MySql;

namespace Squidex.EntityFramework.Migrations;

[Trait("Category", "TestContainer")]
public class MySqlMigrationTests : IAsyncLifetime
{
    private readonly MySqlContainer mysql = new MySqlBuilder("mysql:8.0")
        .WithCommand("--log-bin-trust-function-creators=1")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await mysql.StartAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await mysql.DisposeAsync();
    }

    [Fact]
    public async Task Should_migrate()
    {
        var services =
            new ServiceCollection()
                .AddDbContextFactory<MySqlAppDbContext>(b =>
                {
                    var connectionString = mysql.GetConnectionString();

                    b.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), options =>
                    {
                        options.UseNetTopologySuite();
                        options.UseMicrosoftJson(MySqlCommonJsonChangeTrackingOptions.FullHierarchyOptimizedSemantically);
                    });
                })
                .AddSingleton<ConnectionStringParser, MySqlConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddSingleton<DatabaseMigrator<MySqlAppDbContext>>()
                .BuildServiceProvider();

        var databaseMigrator = services.GetRequiredService<DatabaseMigrator<MySqlAppDbContext>>();
        var databaseFactory = services.GetRequiredService<IDbContextFactory<MySqlAppDbContext>>();

        await databaseMigrator.InitializeAsync(default);

        await using var dbContext = await databaseFactory.CreateDbContextAsync();

        var migrations = await dbContext.Database.GetAppliedMigrationsAsync();
        Assert.NotEmpty(migrations);
    }

    [Fact]
    public async Task Should_migrate_idempotent_and_functions_callable()
    {
        var connectionString = mysql.GetConnectionString();

        var services =
            new ServiceCollection()
                .AddDbContextFactory<MySqlAppDbContext>(b =>
                {
                    b.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), options =>
                    {
                        options.UseNetTopologySuite();
                        options.UseMicrosoftJson(MySqlCommonJsonChangeTrackingOptions.FullHierarchyOptimizedSemantically);
                    });
                })
                .AddSingleton<ConnectionStringParser, MySqlConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddSingleton<DatabaseMigrator<MySqlAppDbContext>>()
                .BuildServiceProvider();

        var databaseMigrator = services.GetRequiredService<DatabaseMigrator<MySqlAppDbContext>>();
        var databaseFactory = services.GetRequiredService<IDbContextFactory<MySqlAppDbContext>>();

        // Run migrations twice to verify idempotency.
        await databaseMigrator.InitializeAsync(default);
        await databaseMigrator.InitializeAsync(default);

        // Verify the json_exists function was created and is callable.
        // Note: {{ and }} are escaped braces for ExecuteSqlRawAsync's string.Format-style parser.
        await using var dbContext = await databaseFactory.CreateDbContextAsync();
        var result = await dbContext.Database.ExecuteSqlRawAsync(
            "SELECT json_exists('{{\"a\":1}}', '$.a')");
        Assert.Equal(-1, result);
    }

    [Fact]
    public async Task Should_migrate_when_functions_already_exist()
    {
        var connectionString = mysql.GetConnectionString();

        var services =
            new ServiceCollection()
                .AddDbContextFactory<MySqlAppDbContext>(b =>
                {
                    b.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), options =>
                    {
                        options.UseNetTopologySuite();
                        options.UseMicrosoftJson(MySqlCommonJsonChangeTrackingOptions.FullHierarchyOptimizedSemantically);
                    });
                })
                .AddSingleton<ConnectionStringParser, MySqlConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddSingleton<DatabaseMigrator<MySqlAppDbContext>>()
                .BuildServiceProvider();

        var databaseMigrator = services.GetRequiredService<DatabaseMigrator<MySqlAppDbContext>>();
        var databaseFactory = services.GetRequiredService<IDbContextFactory<MySqlAppDbContext>>();

        // Simulate a pre-migration database: apply all migrations up to (but not including)
        // AddJsonFunctions, then create the functions via the old SqlDialectInitializer path.
        await using (var dbContext = await databaseFactory.CreateDbContextAsync())
        {
            await dbContext.Database.MigrateAsync("20260323154443_MigrateToNet10");

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
            "SELECT json_exists('{{\"a\":1}}', '$.a')");
        Assert.Equal(-1, result);
    }
}
