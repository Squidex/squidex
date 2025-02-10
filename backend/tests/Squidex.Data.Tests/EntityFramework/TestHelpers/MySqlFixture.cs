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
using Squidex.Providers.MySql;
using Testcontainers.MySql;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.EntityFramework.TestHelpers;

[CollectionDefinition("MySql")]
public sealed class MySqlFixtureCollection : ICollectionFixture<MySqlFixture>
{
}

public sealed class MySqlFixture : IAsyncLifetime, ISqlFixture<TestDbContextMySql>
{
    private readonly MySqlContainer mysql =
        new MySqlBuilder()
            .WithReuse(true)
            .WithLabel("reuse-id", "squidex-mysql")
            .Build();

    private IServiceProvider services;

    public IDbContextFactory<TestDbContextMySql> DbContextFactory => services.GetRequiredService<IDbContextFactory<TestDbContextMySql>>();

    public SqlDialect Dialect => MySqlDialect.Instance;

    public async Task InitializeAsync()
    {
        await mysql.StartAsync();

        services =
            new ServiceCollection()
                 .AddDbContextFactory<TestDbContextMySql>(b =>
                 {
                     var connectionString = mysql.GetConnectionString();

                     b.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mysql =>
                     {
                         mysql.UseMicrosoftJson(MySqlCommonJsonChangeTrackingOptions.FullHierarchyOptimizedSemantically);
                     });
                 })
                 .AddSingleton(TestUtils.DefaultSerializer)
                 .BuildServiceProvider();

        var factory = services.GetRequiredService<IDbContextFactory<TestDbContextMySql>>();
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

        await mysql.StopAsync();
    }
}
