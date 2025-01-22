// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.AI;
using Squidex.Providers.MySql;
using Squidex.Providers.Postgres;
using Squidex.Providers.SqlServer;
using YDotNet.Server.EntityFramework;

namespace Squidex;

public static class ServiceExtensions
{
    public static void AddSquidexEntityFrameworkEventStore(this IServiceCollection services, IConfiguration config)
    {
        config.ConfigureByOption("store:ef:provider", new Alternatives
        {
            ["MySql"] = () =>
            {
                services.AddEntityFrameworkEventStore<MySqlDbContext>(config);
            },
            ["Postgres"] = () =>
            {
                services.AddEntityFrameworkEventStore<PostgresDbContext>(config);
            },
            ["SqlServer"] = () =>
            {
                services.AddEntityFrameworkEventStore<SqlServerDbContext>(config);
            },
        });
    }

    public static void AddSquidexEntityFramework(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetRequiredValue("store:ef:connectionString");

        config.ConfigureByOption("store:ef:provider", new Alternatives
        {
            ["MySql"] = () =>
            {
                services.AddDbContextPool<MySqlDbContext>(builder =>
                {
                    builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                });

                services.AddSquidexEntityFramework<MySqlDbContext>(config);
            },
            ["Postgres"] = () =>
            {
                services.AddDbContextPool<PostgresDbContext>(builder =>
                {
                    builder.UseNpgsql(connectionString);
                });

                services.AddSquidexEntityFramework<PostgresDbContext>(config);
            },
            ["SqlServer"] = () =>
            {
                services.AddDbContextPool<SqlServerDbContext>(builder =>
                {
                    builder.UseSqlServer(connectionString);
                });

                services.AddSquidexEntityFramework<SqlServerDbContext>(config);
            },
        });
    }

    private static void AddSquidexEntityFramework<TContext>(this IServiceCollection services, IConfiguration config) where TContext : AppDbContext
    {
        services.AddYDotNet()
            .AddEntityFrameworkStorage<TContext>();

        services.AddAI()
            .AddEntityFrameworkChatStore<TContext>();

        services.AddMessaging()
            .AddEntityFrameworkDataStore<TContext>(config);

        services.AddOpenIddict()
            .AddCore(builder =>
            {
                builder.UseEntityFrameworkCore()
                    .UseDbContext<TContext>();
            });
    }
}
