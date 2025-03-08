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
using Squidex.Providers.MySql.App;
using Testcontainers.MySql;

namespace Squidex.EntityFramework.Migrations;

[Trait("Category", "TestContainer")]
public class MySqlMigrationTests : IAsyncLifetime
{
    private readonly MySqlContainer mysql = new MySqlBuilder().Build();

    public async Task InitializeAsync()
    {
        await mysql.StartAsync();
    }

    public async Task DisposeAsync()
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
}
