// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Providers.Postgres;
using Squidex.Providers.Postgres.Content;
using Squidex.Providers.SqlServer;
using Testcontainers.PostgreSql;

namespace Squidex.EntityFramework.TestHelpers;

public class PostgresFixture(string? reuseId) : IAsyncLifetime, ISqlContentFixture<TestDbContextPostgres, PostgresContentDbContext>
{
    private readonly PostgreSqlContainer postgreSql =
        new PostgreSqlBuilder()
            .WithImage("postgis/postgis")
            .WithReuse(true)
            .WithLabel("reuse-id", reuseId)
            .Build();

    private IServiceProvider services;

    public IDbContextFactory<TestDbContextPostgres> DbContextFactory
        => services.GetRequiredService<IDbContextFactory<TestDbContextPostgres>>();

    public IDbContextNamedFactory<PostgresContentDbContext> DbContextNamedFactory
        => services.GetRequiredService<IDbContextNamedFactory<PostgresContentDbContext>>();

    static PostgresFixture()
    {
        BulkHelper.Configure();
    }

    public async Task InitializeAsync()
    {
        await postgreSql.StartAsync();

        var connectionString = postgreSql.GetConnectionString();

        services =
            new ServiceCollection()
                .AddDbContextFactory<TestDbContextPostgres>(b =>
                {
                    b.UseNpgsql(connectionString, options =>
                    {
                        options.UseNetTopologySuite();
                    });
                })
                .AddNamedDbContext((jsonSerializer, name) =>
                {
                    return new PostgresContentDbContext(name, connectionString, jsonSerializer);
                })
                .AddSingleton<ConnectionStringParser, PostgresConnectionStringParser>()
                .AddSingletonAs<DatabaseCreator<TestDbContextPostgres>>().Done()
                .AddSingleton(TestUtils.DefaultSerializer)
                .BuildServiceProvider();

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
