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

namespace Squidex.EntityFramework.TestHelpers;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgreSql =
        new PostgreSqlBuilder()
            .WithReuse(true)
            .WithLabel("reuse-id", "squidex-postgres")
            .Build();

    private IServiceProvider services;

    public IDbContextFactory<TestDbContext> DbContextFactory => services.GetRequiredService<IDbContextFactory<TestDbContext>>();

    public SqlDialect Dialect => PostgresDialect.Instance;

    public async Task InitializeAsync()
    {
        await postgreSql.StartAsync();

        services =
            new ServiceCollection()
                 .AddDbContextFactory<TestDbContext>(b =>
                 {
                     b.UseNpgsql(postgreSql.GetConnectionString());
                 })
                 .AddSingleton(TestUtils.DefaultSerializer)
                 .BuildServiceProvider();

        var factory = services.GetRequiredService<IDbContextFactory<TestDbContext>>();
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
