// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Providers.Postgres;
using Squidex.Providers.Postgres.App;
using Testcontainers.PostgreSql;

namespace Squidex.EntityFramework.Migrations;

[Trait("Category", "TestContainer")]
public class PostgresMigrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgreSql =
        new PostgreSqlBuilder("imresamu/postgis:16-3.4")
            .Build();

    public async ValueTask InitializeAsync()
    {
        await postgreSql.StartAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await postgreSql.DisposeAsync();
    }

    [Fact]
    public async Task Should_migrate()
    {
        var services =
            new ServiceCollection()
                .AddDbContextFactory<PostgresAppDbContext>(b =>
                {
                    b.UseNpgsql(postgreSql.GetConnectionString(), options =>
                    {
                        options.UseNetTopologySuite();
                    });
                })
                .AddSingleton<ConnectionStringParser, PostgresConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddLogging()
                .AddSingleton<DatabaseMigrator<PostgresAppDbContext>>()
                .BuildServiceProvider();

        var databaseMigrator = services.GetRequiredService<DatabaseMigrator<PostgresAppDbContext>>();
        var databaseFactory = services.GetRequiredService<IDbContextFactory<PostgresAppDbContext>>();

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
                .AddDbContextFactory<PostgresAppDbContext>(b =>
                {
                    b.UseNpgsql(postgreSql.GetConnectionString(), options =>
                    {
                        options.UseNetTopologySuite();
                    });
                })
                .AddSingleton<ConnectionStringParser, PostgresConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddLogging()
                .AddSingleton<DatabaseMigrator<PostgresAppDbContext>>()
                .BuildServiceProvider();

        var databaseMigrator = services.GetRequiredService<DatabaseMigrator<PostgresAppDbContext>>();
        var databaseFactory = services.GetRequiredService<IDbContextFactory<PostgresAppDbContext>>();

        // Run migrations twice to verify idempotency.
        await databaseMigrator.InitializeAsync(default);
        await databaseMigrator.InitializeAsync(default);

        // Verify the jsonb_exists function was created and is callable.
        // Note: {{ and }} are escaped braces for ExecuteSqlRawAsync's string.Format-style parser.
        await using var dbContext = await databaseFactory.CreateDbContextAsync();
        var result = await dbContext.Database.ExecuteSqlRawAsync(
            "SELECT jsonb_exists('{{\"a\":1}}'::jsonb)");
        Assert.Equal(-1, result);
    }

    [Fact]
    public async Task Should_migrate_when_functions_already_exist()
    {
        var services =
            new ServiceCollection()
                .AddDbContextFactory<PostgresAppDbContext>(b =>
                {
                    b.UseNpgsql(postgreSql.GetConnectionString(), options =>
                    {
                        options.UseNetTopologySuite();
                    });
                })
                .AddSingleton<ConnectionStringParser, PostgresConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddLogging()
                .AddSingleton<DatabaseMigrator<PostgresAppDbContext>>()
                .BuildServiceProvider();

        var databaseMigrator = services.GetRequiredService<DatabaseMigrator<PostgresAppDbContext>>();
        var databaseFactory = services.GetRequiredService<IDbContextFactory<PostgresAppDbContext>>();

        // Simulate a pre-migration database: apply all migrations up to (but not including)
        // AddJsonFunctions, then create the functions via the old SqlDialectInitializer path.
        await using (var dbContext = await databaseFactory.CreateDbContextAsync())
        {
            await dbContext.Database.MigrateAsync("20260323155026_MigrateToNet10");

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
            "SELECT jsonb_exists('{{\"a\":1}}'::jsonb)");
        Assert.Equal(-1, result);
    }

    [Fact]
    public async Task Should_log_script_if_privileges_are_missing()
    {
        var connectionString = await CreateRestrictedDatabaseAsync();

        var log = new TestLogger<DatabaseMigrator<PostgresAppDbContext>>();

        var services =
            new ServiceCollection()
                .AddDbContextFactory<PostgresAppDbContext>(b =>
                {
                    b.UseNpgsql(connectionString, options =>
                    {
                        options.UseNetTopologySuite();
                    });
                })
                .AddSingleton<ConnectionStringParser, PostgresConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddSingleton<ILogger<DatabaseMigrator<PostgresAppDbContext>>>(log)
                .AddSingleton<DatabaseMigrator<PostgresAppDbContext>>()
                .BuildServiceProvider();

        var databaseMigrator = services.GetRequiredService<DatabaseMigrator<PostgresAppDbContext>>();

        // Only a superuser can create the postgis extension.
        var ex = await Assert.ThrowsAsync<PostgresException>(() => databaseMigrator.InitializeAsync(default));
        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, ex.SqlState);

        var message = Assert.Single(log.Messages);
        Assert.Contains("Insufficient privileges", message, StringComparison.Ordinal);
        Assert.Contains("CREATE EXTENSION IF NOT EXISTS postgis", message, StringComparison.Ordinal);
        Assert.Contains("INSERT INTO \"__EFMigrationsHistory\"", message, StringComparison.Ordinal);

        // Execute the script like an administrator with the Postgres client to verify that it is executable.
        // The application user executes it with temporarily elevated privileges to remain the owner of the tables.
        var script = message[(message.IndexOf('\n', StringComparison.Ordinal) + 1)..];

        await postgreSql.CopyAsync(Encoding.UTF8.GetBytes(script), "/tmp/migration.sql", ct: TestContext.Current.CancellationToken);

        await ExecuteAsSuperuserAsync("ALTER ROLE restricted SUPERUSER");

        var execResult = await postgreSql.ExecAsync(["psql", "-v", "ON_ERROR_STOP=1", "-U", "restricted", "-d", "restricted", "-f", "/tmp/migration.sql"], TestContext.Current.CancellationToken);
        Assert.True(execResult.ExitCode == 0, execResult.Stdout + execResult.Stderr);

        await ExecuteAsSuperuserAsync("ALTER ROLE restricted NOSUPERUSER");

        await databaseMigrator.InitializeAsync(default);

        await using var verifyContext = await services.GetRequiredService<IDbContextFactory<PostgresAppDbContext>>().CreateDbContextAsync();
        var result = await verifyContext.Database.ExecuteSqlRawAsync(
            "SELECT jsonb_exists('{{\"a\":1}}'::jsonb)");
        Assert.Equal(-1, result);
    }

    private async Task<string> CreateRestrictedDatabaseAsync()
    {
        await ExecuteAsSuperuserAsync("CREATE ROLE restricted LOGIN PASSWORD 'restricted'");
        await ExecuteAsSuperuserAsync("CREATE DATABASE restricted OWNER restricted");

        var builder = new NpgsqlConnectionStringBuilder(postgreSql.GetConnectionString())
        {
            Database = "restricted",
            Username = "restricted",
            Password = "restricted",
        };

        return builder.ConnectionString;
    }

    private async Task ExecuteAsSuperuserAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(postgreSql.GetConnectionString());
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }
}
