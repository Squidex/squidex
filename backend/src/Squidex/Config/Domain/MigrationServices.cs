// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Migrations;
using Migrations.Migrations;
using Squidex.Infrastructure.Migrations;

namespace Squidex.Config.Domain
{
    public static class MigrationServices
    {
        public static void AddSquidexMigration(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<RebuildOptions>(config,
                "rebuild");

            services.AddSingletonAs<Migrator>()
                .AsSelf();

            services.AddTransientAs<RebuildRunner>()
                .AsSelf();

            services.AddTransientAs<MigrationPath>()
                .As<IMigrationPath>();

            services.AddTransientAs<ConvertEventStore>()
                .As<IMigration>();

            services.AddTransientAs<ConvertEventStoreAppId>()
                .As<IMigration>();

            services.AddTransientAs<ClearRules>()
                .As<IMigration>();

            services.AddTransientAs<ClearSchemas>()
                .As<IMigration>();

            services.AddTransientAs<CreateAssetSlugs>()
                .As<IMigration>();

            services.AddTransientAs<RebuildContents>()
                .As<IMigration>();

            services.AddTransientAs<RebuildSnapshots>()
                .As<IMigration>();

            services.AddTransientAs<RebuildApps>()
                .As<IMigration>();

            services.AddTransientAs<RebuildSchemas>()
                .As<IMigration>();

            services.AddTransientAs<RebuildRules>()
                .As<IMigration>();

            services.AddTransientAs<RebuildAssets>()
                .As<IMigration>();

            services.AddTransientAs<RebuildAssetFolders>()
                .As<IMigration>();

            services.AddTransientAs<StartEventConsumers>()
                .As<IMigration>();

            services.AddTransientAs<StopEventConsumers>()
                .As<IMigration>();
        }
    }
}
