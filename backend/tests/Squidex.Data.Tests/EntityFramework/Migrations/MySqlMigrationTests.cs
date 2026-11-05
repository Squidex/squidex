// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
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

                    b.ReplaceService<IMigrationsSqlGenerator, MySqlScriptMigrationsSqlGenerator>();
                })
                .AddSingleton<ConnectionStringParser, MySqlConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddLogging()
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

                    b.ReplaceService<IMigrationsSqlGenerator, MySqlScriptMigrationsSqlGenerator>();
                })
                .AddSingleton<ConnectionStringParser, MySqlConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddLogging()
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

                    b.ReplaceService<IMigrationsSqlGenerator, MySqlScriptMigrationsSqlGenerator>();
                })
                .AddSingleton<ConnectionStringParser, MySqlConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddLogging()
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

    [Fact]
    public async Task Should_log_script_if_privileges_are_missing()
    {
        // Binary logging is enabled by default, therefore functions can only be created with the SUPER privilege.
        await using var mysqlRestricted = new MySqlBuilder("mysql:8.0").Build();
        await mysqlRestricted.StartAsync(TestContext.Current.CancellationToken);

        var log = new TestLogger<DatabaseMigrator<MySqlAppDbContext>>();

        var services =
            new ServiceCollection()
                .AddDbContextFactory<MySqlAppDbContext>(b =>
                {
                    var connectionString = mysqlRestricted.GetConnectionString();

                    b.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), options =>
                    {
                        options.UseNetTopologySuite();
                        options.UseMicrosoftJson(MySqlCommonJsonChangeTrackingOptions.FullHierarchyOptimizedSemantically);
                    });

                    b.ReplaceService<IMigrationsSqlGenerator, MySqlScriptMigrationsSqlGenerator>();
                })
                .AddSingleton<ConnectionStringParser, MySqlConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddSingleton<ILogger<DatabaseMigrator<MySqlAppDbContext>>>(log)
                .AddSingleton<DatabaseMigrator<MySqlAppDbContext>>()
                .BuildServiceProvider();

        var databaseMigrator = services.GetRequiredService<DatabaseMigrator<MySqlAppDbContext>>();

        var ex = await Assert.ThrowsAsync<MySqlException>(() => databaseMigrator.InitializeAsync(default));
        Assert.Equal(MySqlErrorCode.BinLogCreateRoutineNeedSuper, ex.ErrorCode);

        var message = Assert.Single(log.Messages);
        Assert.Contains("Insufficient privileges", message, StringComparison.Ordinal);
        Assert.Contains("DELIMITER //", message, StringComparison.Ordinal);
        Assert.Contains("CREATE FUNCTION json_empty", message, StringComparison.Ordinal);
        Assert.DoesNotContain("20250204160706_Initial", message, StringComparison.Ordinal);

        // Execute the script like an administrator with the MySQL client to verify that it is executable.
        var script = message[(message.IndexOf('\n', StringComparison.Ordinal) + 1)..];

        await mysqlRestricted.CopyAsync(Encoding.UTF8.GetBytes(script), "/tmp/migration.sql", ct: TestContext.Current.CancellationToken);

        var execResult = await mysqlRestricted.ExecAsync(["sh", "-c", "mysql -uroot -pmysql test < /tmp/migration.sql"], TestContext.Current.CancellationToken);
        Assert.True(execResult.ExitCode == 0, execResult.Stdout + execResult.Stderr);

        await databaseMigrator.InitializeAsync(default);

        await using var verifyContext = await services.GetRequiredService<IDbContextFactory<MySqlAppDbContext>>().CreateDbContextAsync();
        var result = await verifyContext.Database.ExecuteSqlRawAsync(
            "SELECT json_exists('{{\"a\":1}}', '$.a')");
        Assert.Equal(-1, result);
    }
}
