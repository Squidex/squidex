// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using PhenX.EntityFrameworkCore.BulkInsert.MySql;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Providers.MySql;
using Squidex.Providers.MySql.Content;
using Testcontainers.MySql;

namespace Squidex.EntityFramework.TestHelpers;

public class MySqlFixture(string? reuseId = null) : IAsyncLifetime, ISqlContentFixture<TestDbContextMySql, MySqlContentDbContext>
{
    private readonly MySqlContainer mysql =
        new MySqlBuilder("mysql:8.0")
            .WithReuse(true)
            .WithLabel("reuse-id", reuseId)
            .WithCommand("--log-bin-trust-function-creators=1", "--local-infile=1")
            .Build();

    private IServiceProvider services;

    public IDbContextFactory<TestDbContextMySql> DbContextFactory
        => services.GetRequiredService<IDbContextFactory<TestDbContextMySql>>();

    public IDbContextNamedFactory<MySqlContentDbContext> DbContextNamedFactory
        => services.GetRequiredService<IDbContextNamedFactory<MySqlContentDbContext>>();

    public async ValueTask InitializeAsync()
    {
        await mysql.StartAsync(TestContext.Current.CancellationToken);

        var connectionString = $"{mysql.GetConnectionString()};AllowLoadLocalInfile=true;MaxPoolSize=1000";

        services =
            new ServiceCollection()
                .AddPooledDbContextFactory<TestDbContextMySql>(builder =>
                {
                    builder.UseBulkInsertMySql();
                    builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), options =>
                    {
                        options.UseNetTopologySuite();
                        options.UseMicrosoftJson(MySqlCommonJsonChangeTrackingOptions.FullHierarchyOptimizedSemantically);
                    });
                })
                .AddNamedDbContext<MySqlContentDbContext>((builder, name) =>
                {
                    builder.UseBulkInsertMySql();
                    builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), options =>
                    {
                        options.UseMicrosoftJson(MySqlCommonJsonChangeTrackingOptions.FullHierarchyOptimizedSemantically);
                        options.MigrationsHistoryTable($"{name}MigrationHistory");
                    });

                    builder.ConfigureWarnings(w =>
                        w.Ignore(RelationalEventId.PendingModelChangesWarning));
                })
                .AddSingleton<ConnectionStringParser, MySqlConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .BuildServiceProvider();

        foreach (var service in services.GetRequiredService<IEnumerable<IInitializable>>())
        {
            await service.InitializeAsync(default);
        }

        await using var dbContext = await services.GetRequiredService<IDbContextFactory<TestDbContextMySql>>().CreateDbContextAsync();
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

        await mysql.StopAsync(TestContext.Current.CancellationToken);
    }
}
