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
using Squidex.Providers.Postgres;
using Testcontainers.PostgreSql;

namespace Squidex.EntityFramework.Migrations;

[Trait("Category", "TestContainer")]
public class PostgresMigrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgreSql =
        new PostgreSqlBuilder()
            .WithImage("postgis/postgis")
            .Build();

    public async Task InitializeAsync()
    {
        await postgreSql.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await postgreSql.DisposeAsync();
    }

    [Fact]
    public async Task Should_migrate()
    {
        var services =
            new ServiceCollection()
                 .AddDbContextFactory<PostgresDbContext>(b =>
                 {
                     b.UseNpgsql(postgreSql.GetConnectionString(), options =>
                     {
                         options.UseNetTopologySuite();
                     });
                 })
                 .AddSingleton(TestUtils.DefaultSerializer)
                 .AddSingleton<DatabaseMigrator<PostgresDbContext>>()
                 .BuildServiceProvider();

        var databaseMigrator = services.GetRequiredService<DatabaseMigrator<PostgresDbContext>>();
        var databaseFactory = services.GetRequiredService<IDbContextFactory<PostgresDbContext>>();

        await databaseMigrator.InitializeAsync(default);

        await using var dbContext = await databaseFactory.CreateDbContextAsync();

        var migrations = await dbContext.Database.GetAppliedMigrationsAsync();
        Assert.NotEmpty(migrations);
    }
}
