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
using Testcontainers.MsSql;

namespace Squidex.EntityFramework.TestHelpers;

public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer sqlServer =
        new MsSqlBuilder()
            .WithReuse(true)
            .WithLabel("reuse-id", "squidex-mssql")
            .Build();

    private IServiceProvider services;

    public IDbContextFactory<TestDbContexSqlServer> DbContextFactory => services.GetRequiredService<IDbContextFactory<TestDbContexSqlServer>>();

    public async Task InitializeAsync()
    {
        await sqlServer.StartAsync();

        services =
            new ServiceCollection()
                 .AddDbContextFactory<TestDbContexSqlServer>(b =>
                 {
                     b.UseSqlServer(sqlServer.GetConnectionString());
                 })
                 .AddSingleton(TestUtils.DefaultSerializer)
                 .BuildServiceProvider();

        var factory = services.GetRequiredService<IDbContextFactory<TestDbContexSqlServer>>();
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
