// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.AI;
using Squidex.Assets.TusAdapter;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
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
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Messaging;
using Squidex.Providers.MySql;
using Squidex.Providers.Postgres;
using Squidex.Providers.SqlServer;
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
                services.AddDbContextFactory<MySqlDbContext>(builder =>
                {
                    var versionString = config.GetOptionalValue<string>("store:sql:version");

                    var version =
                        !string.IsNullOrWhiteSpace(versionString) ?
                        ServerVersion.Parse(versionString) :
                        ServerVersion.AutoDetect(connectionString);

                    builder.ConfigureWarnings(w => w.Ignore(CoreEventId.CollectionWithoutComparer));
                    builder.UseMySql(connectionString, version, options =>
                    {
                        options.UseNetTopologySuite();
                        options.UseMicrosoftJson(MySqlCommonJsonChangeTrackingOptions.FullHierarchyOptimizedSemantically);
                    });
                });

                services.AddSingleton(typeof(ISnapshotStore<>), typeof(MySqlSnapshotStore<>));
                services.AddSingleton(MySqlDialect.Instance);
                services.AddSquidexEntityFramework<MySqlDbContext>(config);
            },
            ["Postgres"] = () =>
            {
                services.AddDbContextFactory<PostgresDbContext>(builder =>
                {
                    builder.ConfigureWarnings(w => w.Ignore(CoreEventId.CollectionWithoutComparer));
                    builder.UseNpgsql(connectionString, options =>
                    {
                        options.UseNetTopologySuite();
                    });
                });

                services.AddSingleton(typeof(ISnapshotStore<>), typeof(PostgresSnapshotStore<>));
                services.AddSingleton(PostgresDialect.Instance);
                services.AddSquidexEntityFramework<PostgresDbContext>(config);
            },
            ["SqlServer"] = () =>
            {
                services.AddDbContextFactory<SqlServerDbContext>(builder =>
                {
                    builder.ConfigureWarnings(w => w.Ignore(CoreEventId.CollectionWithoutComparer));
                    builder.UseSqlServer(connectionString, options =>
                    {
                        options.UseNetTopologySuite();
                    });
                });

                services.AddSingleton(typeof(ISnapshotStore<>), typeof(SqlServerSnapshotStore<>));
                services.AddSingleton(SqlServerDialect.Instance);
                services.AddSquidexEntityFramework<SqlServerDbContext>(config);
            },
        });
    }

    private static void AddSquidexEntityFramework<TContext>(this IServiceCollection services, IConfiguration config) where TContext : AppDbContext
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

        services.AddSingletonAs<EFContentRepository<TContext>>()
            .As<IContentRepository>().As<ISnapshotStore<WriteContent>>().As<IDeleter>();

        services.AddSingletonAs<EFHistoryEventRepository<TContext>>()
            .As<IHistoryEventRepository>().As<IDeleter>();

        services.AddSingletonAs<EFRequestLogRepository<TContext>>()
            .As<IRequestLogRepository>();

        services.AddSingletonAs<EFRuleRepository<TContext>>()
            .As<IRuleRepository>().As<ISnapshotStore<Rule>>().As<IDeleter>();

        services.AddSingletonAs<EFRuleEventRepository<TContext>>()
            .As<IRuleEventRepository>().As<IDeleter>();

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

        services.AddEntityFrameworkAssetKeyValueStore<TContext, TusMetadata>();
    }

    public static void AddSquidexEntityFrameworkEventStore(this IServiceCollection services, IConfiguration config)
    {
        config.ConfigureByOption("store:sql:provider", new Alternatives
        {
            ["MySql"] = () =>
            {
                services.AddEntityFrameworkEventStore<MySqlDbContext>(config)
                    .AddMysqlAdapter();
            },
            ["Postgres"] = () =>
            {
                services.AddEntityFrameworkEventStore<PostgresDbContext>(config)
                    .AddPostgresAdapter();
            },
            ["SqlServer"] = () =>
            {
                services.AddEntityFrameworkEventStore<SqlServerDbContext>(config)
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
                messaging.AddEntityFrameworkTransport<MySqlDbContext>(config);
            },
            ["Postgres"] = () =>
            {
                messaging.AddEntityFrameworkTransport<PostgresDbContext>(config);
            },
            ["SqlServer"] = () =>
            {
                messaging.AddEntityFrameworkTransport<SqlServerDbContext>(config);
            },
        });

        return messaging;
    }

    public class MySqlSnapshotStore<T>(IDbContextFactory<MySqlDbContext> dbContextFactory)
        : EFSnapshotStore<MySqlDbContext, T, EFState<T>>(dbContextFactory)
    {
    }

    public class PostgresSnapshotStore<T>(IDbContextFactory<PostgresDbContext> dbContextFactory)
        : EFSnapshotStore<PostgresDbContext, T, EFState<T>>(dbContextFactory)
    {
    }

    public class SqlServerSnapshotStore<T>(IDbContextFactory<SqlServerDbContext> dbContextFactory)
        : EFSnapshotStore<SqlServerDbContext, T, EFState<T>>(dbContextFactory)
    {
    }

    public static IdentityBuilder AddIdentityCore(this IServiceCollection services)
    {
        return new IdentityBuilder(typeof(IdentityUser), typeof(IdentityRole), services);
    }
}
