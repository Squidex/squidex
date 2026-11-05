// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
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

                    b.ReplaceService<IMigrationsSqlGenerator, SqlServerScriptMigrationsSqlGenerator>();
                })
                .AddSingleton<ConnectionStringParser, SqlServerConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddLogging()
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

                    b.ReplaceService<IMigrationsSqlGenerator, SqlServerScriptMigrationsSqlGenerator>();
                })
                .AddSingleton<ConnectionStringParser, SqlServerConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddLogging()
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

                    b.ReplaceService<IMigrationsSqlGenerator, SqlServerScriptMigrationsSqlGenerator>();
                })
                .AddSingleton<ConnectionStringParser, SqlServerConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddLogging()
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

    [Fact]
    public async Task Should_log_script_if_privileges_are_missing()
    {
        var connectionString = await CreateRestrictedDatabaseAsync();

        var log = new TestLogger<DatabaseMigrator<SqlServerAppDbContext>>();

        var services =
            new ServiceCollection()
                .AddDbContextFactory<SqlServerAppDbContext>(b =>
                {
                    b.UseSqlServer(connectionString, options =>
                    {
                        options.UseNetTopologySuite();
                    });

                    b.ReplaceService<IMigrationsSqlGenerator, SqlServerScriptMigrationsSqlGenerator>();
                })
                .AddSingleton<ConnectionStringParser, SqlServerConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddSingleton<ILogger<DatabaseMigrator<SqlServerAppDbContext>>>(log)
                .AddSingleton<DatabaseMigrator<SqlServerAppDbContext>>()
                .BuildServiceProvider();

        var databaseMigrator = services.GetRequiredService<DatabaseMigrator<SqlServerAppDbContext>>();

        // The user can create tables, but not functions.
        var ex = await Assert.ThrowsAsync<SqlException>(() => databaseMigrator.InitializeAsync(default));
        Assert.Equal(262, ex.Number);

        var message = Assert.Single(log.Messages);
        Assert.Contains("Insufficient privileges", message, StringComparison.Ordinal);
        Assert.Contains("CREATE OR ALTER FUNCTION dbo.json_empty", message, StringComparison.Ordinal);
        Assert.DoesNotContain("20250204160718_Initial", message, StringComparison.Ordinal);

        // Execute the script like an administrator with the SQL Server client to verify that it is executable.
        var script = message[(message.IndexOf('\n', StringComparison.Ordinal) + 1)..];

        var execResult = await sqlServer.ExecScriptAsync($"USE restricted;\nGO\n{script}", TestContext.Current.CancellationToken);
        Assert.True(execResult.ExitCode == 0, execResult.Stdout + execResult.Stderr);

        await databaseMigrator.InitializeAsync(default);

        await using var verifyContext = await services.GetRequiredService<IDbContextFactory<SqlServerAppDbContext>>().CreateDbContextAsync();
        var result = await verifyContext.Database.ExecuteSqlRawAsync(
            "SELECT dbo.json_exists('{{\"a\":1}}', '$.a')");
        Assert.Equal(-1, result);
    }

    private async Task<string> CreateRestrictedDatabaseAsync()
    {
        const string Password = "Sq1dex_Migr4tion!";

        await using (var connection = new SqlConnection(sqlServer.GetConnectionString()))
        {
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();

            command.CommandText = $"CREATE DATABASE restricted; CREATE LOGIN restricted WITH PASSWORD = '{Password}';";
            await command.ExecuteNonQueryAsync();

            command.CommandText = @"
USE restricted;
CREATE USER restricted FOR LOGIN restricted;
ALTER ROLE db_datareader ADD MEMBER restricted;
ALTER ROLE db_datawriter ADD MEMBER restricted;
GRANT CREATE TABLE TO restricted;
GRANT ALTER, REFERENCES, EXECUTE ON SCHEMA::dbo TO restricted;";
            await command.ExecuteNonQueryAsync();
        }

        var builder = new SqlConnectionStringBuilder(sqlServer.GetConnectionString())
        {
            InitialCatalog = "restricted",
            UserID = "restricted",
            Password = Password,
        };

        return builder.ConnectionString;
    }
}
