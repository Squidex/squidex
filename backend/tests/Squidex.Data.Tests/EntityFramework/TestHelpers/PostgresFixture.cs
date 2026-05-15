// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using PhenX.EntityFrameworkCore.BulkInsert.PostgreSql;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Providers.Postgres;
using Squidex.Providers.Postgres.Content;
using Testcontainers.PostgreSql;

namespace Squidex.EntityFramework.TestHelpers;

public class PostgresFixture(string? reuseId) : IAsyncLifetime, ISqlContentFixture<TestDbContextPostgres, PostgresContentDbContext>
{
    private readonly PostgreSqlContainer postgreSql =
        new PostgreSqlBuilder("imresamu/postgis:16-3.4")
            .WithReuse(true)
            .WithLabel("reuse-id", reuseId)
            .Build();

    private IServiceProvider services;

    public IDbContextFactory<TestDbContextPostgres> DbContextFactory
        => services.GetRequiredService<IDbContextFactory<TestDbContextPostgres>>();

    public IDbContextNamedFactory<PostgresContentDbContext> DbContextNamedFactory
        => services.GetRequiredService<IDbContextNamedFactory<PostgresContentDbContext>>();

    public async ValueTask InitializeAsync()
    {
        await postgreSql.StartAsync(TestContext.Current.CancellationToken);

        var connectionString = postgreSql.GetConnectionString();

        services =
            new ServiceCollection()
                .AddPooledDbContextFactory<TestDbContextPostgres>(builder =>
                {
                    builder.UseBulkInsertPostgreSql();
                    builder.UseNpgsql(connectionString, options =>
                    {
                        options.UseNetTopologySuite();
                    });
                })
                .AddNamedDbContext<PostgresContentDbContext>((builder, name) =>
                {
                    builder.UseBulkInsertPostgreSql();
                    builder.UseNpgsql(connectionString, options =>
                    {
                        options.MigrationsHistoryTable($"{name}MigrationHistory");
                    });

                    builder.ConfigureWarnings(w =>
                        w.Ignore(RelationalEventId.PendingModelChangesWarning));
                })
                .AddSingleton<ConnectionStringParser, PostgresConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .BuildServiceProvider();

        foreach (var service in services.GetRequiredService<IEnumerable<IInitializable>>())
        {
            await service.InitializeAsync(default);
        }

        await using var dbContext = await services.GetRequiredService<IDbContextFactory<TestDbContextPostgres>>().CreateDbContextAsync();
        await dbContext.Database.EnsureCreatedAsync();
        if (dbContext is IDbContextWithDialect withDialect)
        {
            await withDialect.Dialect.InitializeAsync(dbContext, default);
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var service in services.GetRequiredService<IEnumerable<IInitializable>>())
        {
            await service.ReleaseAsync(default);
        }

        await postgreSql.StopAsync(TestContext.Current.CancellationToken);
    }
}
