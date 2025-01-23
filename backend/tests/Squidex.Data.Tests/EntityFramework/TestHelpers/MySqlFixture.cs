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
using Testcontainers.MySql;
using Testcontainers.PostgreSql;

namespace Squidex.EntityFramework.TestHelpers;

public sealed class MySqlFixture : IAsyncLifetime
{
    private readonly MySqlContainer mysql =
        new MySqlBuilder()
            .WithReuse(true)
            .WithLabel("reuse-id", "squidex-mysql")
            .Build();

    private IServiceProvider services;

    public IDbContextFactory<TestDbContext> DbContextFactory => services.GetRequiredService<IDbContextFactory<TestDbContext>>();

    public async Task InitializeAsync()
    {
        await mysql.StartAsync();

        services =
            new ServiceCollection()
                 .AddDbContextFactory<TestDbContext>(b =>
                 {
                     b.UseMySql(mysql.GetConnectionString(), ServerVersion.AutoDetect(mysql.GetConnectionString()), mysql =>
                     {
                         mysql.UseMicrosoftJson(MySqlCommonJsonChangeTrackingOptions.FullHierarchyOptimizedSemantically);
                     });
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

        await mysql.StopAsync();
    }
}
