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
using Squidex.Providers.MySql;
using Squidex.Providers.MySql.App;
using Testcontainers.MySql;

namespace Squidex.EntityFramework.Migrations;

[Trait("Category", "TestContainer")]
public class MySqlMigrationTests : IAsyncLifetime
{
    private readonly MySqlContainer mysql =
        new MySqlBuilder("mysql:8.0")
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
        var result = await ExecuteScalarAsync(dbContext, "SELECT json_empty(NULL, '$')");

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
