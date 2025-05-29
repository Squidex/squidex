﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhenX.EntityFrameworkCore.BulkInsert.MySql;
using PhenX.EntityFrameworkCore.BulkInsert.PostgreSql;
using PhenX.EntityFrameworkCore.BulkInsert.SqlServer;
using Squidex.Assets.TusAdapter;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Domain.Apps.Entities.Teams.Repositories;
using Squidex.Domain.Users;
using Squidex.Events.EntityFramework;
using Squidex.Flows.EntityFramework;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Messaging;
using Squidex.Providers.MySql;
using Squidex.Providers.MySql.App;
using Squidex.Providers.MySql.Content;
using Squidex.Providers.Postgres;
using Squidex.Providers.Postgres.App;
using Squidex.Providers.Postgres.Content;
using Squidex.Providers.SqlServer;
using Squidex.Providers.SqlServer.App;
using Squidex.Providers.SqlServer.Content;
using YDotNet.Server.EntityFramework;

namespace Squidex;

public static class ServiceExtensions
{
    public static void AddSquidexEntityFramework(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetRequiredValue("store:sql:connectionString");

        config.ConfigureByOption("store:sql:provider", new Alternatives
        {
            ["MySql"] = () =>
            {
                var versionString = config.GetOptionalValue<string>("store:sql:version");

                var version =
                    !string.IsNullOrWhiteSpace(versionString) ?
                    ServerVersion.Parse(versionString) :
                    ServerVersion.AutoDetect(connectionString);

                services.AddPooledDbContextFactory<MySqlAppDbContext>(builder =>
                {
                    builder.SetDefaultWarnings();
                    builder.UseMySql(connectionString, version, options =>
                    {
                        options.UseNetTopologySuite();
                        options.UseMicrosoftJson(MySqlCommonJsonChangeTrackingOptions.FullHierarchyOptimizedSemantically);
                    });
                    builder.UseBulkInsertMySql();
                });

                services.AddNamedDbContext<MySqlContentDbContext>((builder, name) =>
                {
                    builder.SetDefaultWarnings();
                    builder.UseBulkInsertMySql();
                    builder.UseMySql(connectionString, version, options =>
                    {
                        options.MigrationsHistoryTable($"{name}MigrationHistory");
                        options.UseMicrosoftJson(MySqlCommonJsonChangeTrackingOptions.FullHierarchyOptimizedSemantically);
                    });
                });

                services.AddSingleton(typeof(ISnapshotStore<>), typeof(MySqlSnapshotStore<>));
                services.AddSquidexEntityFramework<MySqlAppDbContext, MySqlContentDbContext>(config);

                services.AddSingletonAs<MySqlDiagnostics>()
                    .As<ITelemetryConfigurator>();

                services.AddSingletonAs<MySqlConnectionStringParser>()
                    .As<ConnectionStringParser>();
            },
            ["Postgres"] = () =>
            {
                services.AddPooledDbContextFactory<PostgresAppDbContext>(builder =>
                {
                    builder.SetDefaultWarnings();
                    builder.UseBulkInsertPostgreSql();
                    builder.UseNpgsql(connectionString, options =>
                    {
                        options.UseNetTopologySuite();
                    });
                });

                services.AddNamedDbContext<PostgresContentDbContext>((builder, name) =>
                {
                    builder.SetDefaultWarnings();
                    builder.UseBulkInsertPostgreSql();
                    builder.UseNpgsql(connectionString, options =>
                    {
                        options.MigrationsHistoryTable($"{name}MigrationHistory");
                    });
                });

                services.AddSingleton(typeof(ISnapshotStore<>), typeof(PostgresSnapshotStore<>));
                services.AddSquidexEntityFramework<PostgresAppDbContext, PostgresContentDbContext>(config);

                services.AddSingletonAs<PostgresDiagnostics>()
                    .As<ITelemetryConfigurator>();

                services.AddSingletonAs<PostgresConnectionStringParser>()
                    .As<ConnectionStringParser>();
            },
            ["SqlServer"] = () =>
            {
                services.AddPooledDbContextFactory<SqlServerAppDbContext>(builder =>
                {
                    builder.SetDefaultWarnings();
                    builder.UseSqlServer(connectionString, options =>
                    {
                        options.UseNetTopologySuite();
                    });
                    builder.UseBulkInsertSqlServer();
                });

                services.AddNamedDbContext<SqlServerContentDbContext>((builder, name) =>
                {
                    builder.SetDefaultWarnings();
                    builder.UseBulkInsertSqlServer();
                    builder.UseSqlServer(connectionString, options =>
                    {
                        options.MigrationsHistoryTable($"{name}MigrationHistory");
                    });
                });

                services.AddSingleton(typeof(ISnapshotStore<>), typeof(SqlServerSnapshotStore<>));
                services.AddSquidexEntityFramework<SqlServerAppDbContext, SqlServerContentDbContext>(config);

                services.AddSingletonAs<SqlServerDiagnostics>()
                    .As<ITelemetryConfigurator>();

                services.AddSingletonAs<SqlServerConnectionStringParser>()
                    .As<ConnectionStringParser>();
            },
        });
    }

    private static void AddSquidexEntityFramework<TContext, TContextContext>(this IServiceCollection services, IConfiguration config)
        where TContext : AppDbContext
        where TContextContext : ContentDbContext
    {
        if (config.GetValue<bool>("store:sql:runMigration"))
        {
            services.AddSingletonAs<DatabaseMigrator<TContext>>();
        }

        services.AddOpenIddict()
            .AddCore(builder =>
            {
                builder.UseEntityFrameworkCore()
                    .UseDbContext<TContext>();
            });

        services.AddIdentityCore()
            .AddEntityFrameworkStores<TContext>();

        services.AddYDotNet()
            .AddEntityFrameworkStorage<TContext>();

        services.AddAI()
            .AddEntityFrameworkChatStore<TContext>();

        services.AddMessaging()
            .AddEntityFrameworkDataStore<TContext>(config);

        services.AddSingletonAs<EFMigrationStatus<TContext>>()
            .As<IMigrationStatus>();

        services.AddSingletonAs<EFDistributedCache<TContext>>()
            .As<IDistributedCache>();

        services.AddHealthChecks()
            .AddDbContextCheck<TContext>();

        services.AddSingletonAs<EFAppRepository<TContext>>()
            .As<IAppRepository>().As<ISnapshotStore<App>>();

        services.AddSingletonAs<EFAssetRepository<TContext>>()
            .As<IAssetRepository>().As<ISnapshotStore<Asset>>().As<IDeleter>();

        services.AddSingletonAs<EFAssetFolderRepository<TContext>>()
            .As<IAssetFolderRepository>().As<ISnapshotStore<AssetFolder>>().As<IDeleter>();

        services.AddSingletonAs<EFContentRepository<TContext, TContextContext>>()
            .As<IContentRepository>().As<ISnapshotStore<WriteContent>>().As<IDeleter>();

        services.AddSingletonAs<EFHistoryEventRepository<TContext>>()
            .As<IHistoryEventRepository>().As<IDeleter>();

        services.AddSingletonAs<EFRequestLogRepository<TContext>>()
            .As<IRequestLogRepository>();

        services.AddSingletonAs<EFRuleRepository<TContext>>()
            .As<IRuleRepository>().As<ISnapshotStore<Rule>>().As<IDeleter>();

        services.AddSingletonAs<EFSchemaRepository<TContext>>()
            .As<ISchemaRepository>().As<ISnapshotStore<Schema>>().As<IDeleter>().As<ISchemasHash>();

        services.AddSingletonAs<EFTeamRepository<TContext>>()
            .As<ITeamRepository>().As<ISnapshotStore<Team>>();

        services.AddSingletonAs<EFTextIndexerState<TContext>>()
            .As<ITextIndexerState>().As<IDeleter>();

        services.AddSingletonAs<EFTextIndex<TContext>>()
            .As<ITextIndex>().As<IDeleter>();

        services.AddSingletonAs<EFUsageRepository<TContext>>()
            .As<IUsageRepository>();

        services.AddSingletonAs<EFUserFactory>()
            .As<IUserFactory>();

        services.AddSingletonAs<BulkInserter>()
            .As<IDbEventStoreBulkInserter>().As<IDbFlowsBulkInserter>();

        services.AddFlowsCore()
            .AddEntityFrameworkStore<TContext, FlowEventContext>();

        services.AddCronJobsCore()
            .AddEntityFrameworkStore<TContext, CronJobContext>();

        services.AddEntityFrameworkAssetKeyValueStore<TContext, TusMetadata>();
    }

    public static void AddSquidexEntityFrameworkEventStore(this IServiceCollection services, IConfiguration config)
    {
        config.ConfigureByOption("store:sql:provider", new Alternatives
        {
            ["MySql"] = () =>
            {
                services.AddEntityFrameworkEventStore<MySqlAppDbContext>(config)
                    .AddMysqlAdapter();
            },
            ["Postgres"] = () =>
            {
                services.AddEntityFrameworkEventStore<PostgresAppDbContext>(config)
                    .AddPostgresAdapter();
            },
            ["SqlServer"] = () =>
            {
                services.AddEntityFrameworkEventStore<SqlServerAppDbContext>(config)
                    .AddSqlServerAdapter();
            },
        });
    }

    public static MessagingBuilder AddSquidexEntityFrameworkTransport(this MessagingBuilder messaging, IConfiguration config)
    {
        config.ConfigureByOption("store:sql:provider", new Alternatives
        {
            ["MySql"] = () =>
            {
                messaging.AddEntityFrameworkTransport<MySqlAppDbContext>(config);
            },
            ["Postgres"] = () =>
            {
                messaging.AddEntityFrameworkTransport<PostgresAppDbContext>(config);
            },
            ["SqlServer"] = () =>
            {
                messaging.AddEntityFrameworkTransport<SqlServerAppDbContext>(config);
            },
        });

        return messaging;
    }

    public class MySqlSnapshotStore<T>(IDbContextFactory<MySqlAppDbContext> dbContextFactory)
        : EFSnapshotStore<MySqlAppDbContext, T, EFState<T>>(dbContextFactory)
    {
    }

    public class PostgresSnapshotStore<T>(IDbContextFactory<PostgresAppDbContext> dbContextFactory)
        : EFSnapshotStore<PostgresAppDbContext, T, EFState<T>>(dbContextFactory)
    {
    }

    public class SqlServerSnapshotStore<T>(IDbContextFactory<SqlServerAppDbContext> dbContextFactory)
        : EFSnapshotStore<SqlServerAppDbContext, T, EFState<T>>(dbContextFactory)
    {
    }

    public static IdentityBuilder AddIdentityCore(this IServiceCollection services)
    {
        return new IdentityBuilder(typeof(IdentityUser), typeof(IdentityRole), services);
    }
}
