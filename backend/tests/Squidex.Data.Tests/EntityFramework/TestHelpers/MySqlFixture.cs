// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
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
        new MySqlBuilder()
            .WithReuse(true)
            .WithLabel("reuse-id", reuseId)
            .WithCommand("--log-bin-trust-function-creators=1", "--local-infile=1")
            .Build();

    private IServiceProvider services;

    public IDbContextFactory<TestDbContextMySql> DbContextFactory
        => services.GetRequiredService<IDbContextFactory<TestDbContextMySql>>();

    public IDbContextNamedFactory<MySqlContentDbContext> DbContextNamedFactory
        => services.GetRequiredService<IDbContextNamedFactory<MySqlContentDbContext>>();

    public async Task InitializeAsync()
    {
        await mysql.StartAsync();

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
                    });
                })
                .AddSingleton<ConnectionStringParser, MySqlConnectionStringParser>()
                .AddSingletonAs<DatabaseCreator<TestDbContextMySql>>().Done()
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

        await mysql.StopAsync();
    }
}
