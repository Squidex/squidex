// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using PhenX.EntityFrameworkCore.BulkInsert.SqlServer;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Providers.SqlServer;
using Squidex.Providers.SqlServer.Content;
using Testcontainers.MsSql;

namespace Squidex.EntityFramework.TestHelpers;

public class SqlServerFixture(string? reuseId = null) : IAsyncLifetime, ISqlContentFixture<TestDbContextSqlServer, SqlServerContentDbContext>
{
    private readonly MsSqlContainer sqlServer =
        new MsSqlBuilder("vibs2006/sql_server_fts")
            .WithReuse(true)
            .WithLabel("reuse-id", reuseId)
            .Build();

    private IServiceProvider services;

    public IDbContextFactory<TestDbContextSqlServer> DbContextFactory
        => services.GetRequiredService<IDbContextFactory<TestDbContextSqlServer>>();

    public IDbContextNamedFactory<SqlServerContentDbContext> DbContextNamedFactory
        => services.GetRequiredService<IDbContextNamedFactory<SqlServerContentDbContext>>();

    public async ValueTask InitializeAsync()
    {
        await sqlServer.StartAsync(TestContext.Current.CancellationToken);
        await sqlServer.ExecScriptAsync($"create database squidex;", TestContext.Current.CancellationToken);

        var connectionString = GetConnectionString();

        services =
            new ServiceCollection()
                .AddPooledDbContextFactory<TestDbContextSqlServer>(builder =>
                {
                    builder.UseBulkInsertSqlServer();
                    builder.UseSqlServer(connectionString, options =>
                    {
                        options.UseNetTopologySuite();
                    });
                })
                .AddNamedDbContext<SqlServerContentDbContext>((builder, name) =>
                {
                    builder.UseBulkInsertSqlServer();
                    builder.UseSqlServer(connectionString, options =>
                    {
                        options.MigrationsHistoryTable($"{name}MigrationHistory");
                    });

                    builder.ConfigureWarnings(w =>
                        w.Ignore(RelationalEventId.PendingModelChangesWarning));
                })
                .AddSingleton<ConnectionStringParser, SqlServerConnectionStringParser>()
                .AddSingleton(TestUtils.DefaultSerializer)
                .BuildServiceProvider();

        foreach (var service in services.GetRequiredService<IEnumerable<IInitializable>>())
        {
            await service.InitializeAsync(default);
        }

        await using var dbContext = await services.GetRequiredService<IDbContextFactory<TestDbContextSqlServer>>().CreateDbContextAsync();
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

        await sqlServer.StopAsync(TestContext.Current.CancellationToken);
    }

    private string GetConnectionString()
    {
        var builder = new SqlConnectionStringBuilder(sqlServer.GetConnectionString())
        {
            InitialCatalog = "squidex",
        };

        return builder.ConnectionString;
    }
}
