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
        var result = await ExecuteScalarAsync(dbContext, "SELECT CAST(dbo.json_empty(NULL, '$') AS INT)");

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
