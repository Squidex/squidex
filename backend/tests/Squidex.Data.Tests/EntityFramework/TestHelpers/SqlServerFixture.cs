// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
        new MsSqlBuilder()
            .WithImage("vibs2006/sql_server_fts")
            .WithReuse(true)
            .WithLabel("reuse-id", reuseId)
            .Build();

    private IServiceProvider services;

    public IDbContextFactory<TestDbContextSqlServer> DbContextFactory
        => services.GetRequiredService<IDbContextFactory<TestDbContextSqlServer>>();

    public IDbContextNamedFactory<SqlServerContentDbContext> DbContextNamedFactory
        => services.GetRequiredService<IDbContextNamedFactory<SqlServerContentDbContext>>();

    public async Task InitializeAsync()
    {
        await sqlServer.StartAsync();
        await sqlServer.ExecScriptAsync($"create database squidex;");

        var connectionString = GetConnectionString();

        services =
            new ServiceCollection()
                .AddDbContextFactory<TestDbContextSqlServer>(builder =>
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
                    builder.UseSqlServer(connectionString);
                })
                .AddSingleton<ConnectionStringParser, SqlServerConnectionStringParser>()
                .AddSingletonAs<DatabaseCreator<TestDbContextSqlServer>>().Done()
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

        await sqlServer.StopAsync();
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
