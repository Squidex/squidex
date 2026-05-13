// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Data;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Migrations;
using Squidex.Providers.Postgres;
using Squidex.Providers.Postgres.App;
using Testcontainers.PostgreSql;

namespace Squidex.EntityFramework.Migrations;

[Trait("Category", "TestContainer")]
public class PostgresMigrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgreSql =
        new PostgreSqlBuilder("postgis/postgis")
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
                .AddSingleton<DatabaseMigrator<PostgresAppDbContext>>()
                .BuildServiceProvider();

        var databaseMigrator = services.GetRequiredService<DatabaseMigrator<PostgresAppDbContext>>();
        var databaseFactory = services.GetRequiredService<IDbContextFactory<PostgresAppDbContext>>();

        await databaseMigrator.InitializeAsync(default);

        await using var dbContext = await databaseFactory.CreateDbContextAsync();

        var migrations = await dbContext.Database.GetAppliedMigrationsAsync();
        var result = await ExecuteScalarAsync(dbContext, "SELECT CASE WHEN jsonb_empty(NULL) THEN 1 ELSE 0 END");

        Assert.NotEmpty(migrations);
        Assert.Equal(1, result);
    }

    private static async Task<int> ExecuteScalarAsync(DbContext dbContext, string sql)
    {
        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }
}
