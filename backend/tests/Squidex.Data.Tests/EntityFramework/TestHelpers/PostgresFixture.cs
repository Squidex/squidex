// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Hosting;
using Squidex.Infrastructure.Queries;
using Squidex.Providers.Postgres;
using Testcontainers.PostgreSql;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable MA0048 // File name must match type name

namespace Squidex.EntityFramework.TestHelpers;

[CollectionDefinition("Postgres")]
public sealed class PostgresFixtureCollection : ICollectionFixture<PostgresFixture>
{
}

public sealed class PostgresFixture : IAsyncLifetime, ISqlFixture<TestDbContextPostgres>
{
    private readonly PostgreSqlContainer postgreSql =
        new PostgreSqlBuilder()
            .WithImage("postgis/postgis")
            .WithReuse(true)
            .WithLabel("reuse-id", "squidex-postgres")
            .Build();

    private IServiceProvider services;

    public IDbContextFactory<TestDbContextPostgres> DbContextFactory => services.GetRequiredService<IDbContextFactory<TestDbContextPostgres>>();

    public SqlDialect Dialect => PostgresDialect.Instance;

    public async Task InitializeAsync()
    {
        await postgreSql.StartAsync();

        services =
            new ServiceCollection()
                 .AddDbContextFactory<TestDbContextPostgres>(b =>
                 {
                     b.UseNpgsql(postgreSql.GetConnectionString(), options =>
                     {
                         options.UseNetTopologySuite();
                     });
                 })
                 .AddSingleton(TestUtils.DefaultSerializer)
                 .BuildServiceProvider();

        var factory = services.GetRequiredService<IDbContextFactory<TestDbContextPostgres>>();
        var context = await factory.CreateDbContextAsync();
        var creator = (RelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>();

        await creator.EnsureCreatedAsync();

        foreach (var service in services.GetRequiredService<IEnumerable<IInitializable>>())
        {
            await service.InitializeAsync(default);
        }
    }

    public async Task DisposeAsync()
    {
        foreach (var service in services.GetRequiredService<IEnumerable<IInitializable>>())
        {
            await service.ReleaseAsync(default);
        }

        await postgreSql.StopAsync();
    }
}
