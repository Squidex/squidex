// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
    private readonly MsSqlContainer sqlServer = new MsSqlBuilder().Build();

    public async Task InitializeAsync()
    {
        await sqlServer.StartAsync();
    }

    public async Task DisposeAsync()
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
}
