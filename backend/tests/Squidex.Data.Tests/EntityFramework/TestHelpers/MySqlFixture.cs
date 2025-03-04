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
using Squidex.Infrastructure.Queries;
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

    public SqlDialect Dialect => MySqlDialect.Instance;

    public async Task InitializeAsync()
    {
        BulkHelper.Configure();

        await mysql.StartAsync();

        var connectionString = $"{mysql.GetConnectionString()};AllowLoadLocalInfile=true";

        services =
            new ServiceCollection()
                .AddDbContextFactory<TestDbContextMySql>(b =>
                {
                    b.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), options =>
                    {
                        options.UseNetTopologySuite();
                        options.UseMicrosoftJson(MySqlCommonJsonChangeTrackingOptions.FullHierarchyOptimizedSemantically);
                    });
                })
                .AddNamedDbContext((jsonSerializer, name) =>
                {
                    return new MySqlContentDbContext(name, connectionString, jsonSerializer);
                })
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
