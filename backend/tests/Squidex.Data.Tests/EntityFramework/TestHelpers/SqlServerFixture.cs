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
using Squidex.Providers.SqlServer;
using Testcontainers.MsSql;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.EntityFramework.TestHelpers;

[CollectionDefinition("SqlServer")]
public sealed class SqlServerFixtureCollection : ICollectionFixture<SqlServerFixture>
{
}

public sealed class SqlServerFixture : IAsyncLifetime, ISqlFixture<TestDbContextSqlServer>
{
    private readonly MsSqlContainer sqlServer =
        new MsSqlBuilder()
            .WithReuse(true)
            .WithLabel("reuse-id", "squidex-mssql")
            .Build();

    private IServiceProvider services;

    public IDbContextFactory<TestDbContextSqlServer> DbContextFactory => services.GetRequiredService<IDbContextFactory<TestDbContextSqlServer>>();

    public SqlDialect Dialect => SqlServerDialect.Instance;

    public async Task InitializeAsync()
    {
        await sqlServer.StartAsync();

        services =
            new ServiceCollection()
                 .AddDbContextFactory<TestDbContextSqlServer>(b =>
                 {
                     b.UseSqlServer(sqlServer.GetConnectionString());
                 })
                 .AddSingleton(TestUtils.DefaultSerializer)
                 .BuildServiceProvider();

        var factory = services.GetRequiredService<IDbContextFactory<TestDbContextSqlServer>>();
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

        await sqlServer.StopAsync();
    }
}
