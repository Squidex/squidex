// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Migrate_01;
using Migrate_01.Migrations;
using Orleans;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Apps.Templates;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Edm;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Domain.Apps.Entities.Tags;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Migrations;
using Squidex.Pipeline;
using Squidex.Pipeline.CommandMiddlewares;

namespace Squidex.Config.Domain
{
    public static class EntitiesServices
    {
        public static void AddMyEntitiesServices(this IServiceCollection services, IConfiguration config)
        {
            var exposeSourceUrl = config.GetOptionalValue("assetStore:exposeSourceUrl", true);

            services.AddSingletonAs(c => new UrlGenerator(
                    c.GetRequiredService<IOptions<MyUrlsOptions>>(),
                    c.GetRequiredService<IAssetStore>(),
                    exposeSourceUrl))
                .As<IGraphQLUrlGenerator>()
                .As<IRuleUrlGenerator>();

            services.AddSingletonAs<CachingGraphQLService>()
                .As<IGraphQLService>();

            services.AddSingletonAs<TempFolderBackupArchiveLocation>()
                .As<IBackupArchiveLocation>();

            services.AddSingletonAs<AppProvider>()
                .As<IAppProvider>();

            services.AddSingletonAs<AssetQueryService>()
                .As<IAssetQueryService>();

            services.AddSingletonAs<ContentQueryService>()
                .As<IContentQueryService>();

            services.AddSingletonAs<ContentVersionLoader>()
                .As<IContentVersionLoader>();

            services.AddSingletonAs<AppHistoryEventsCreator>()
                .As<IHistoryEventsCreator>();

            services.AddSingletonAs<ContentHistoryEventsCreator>()
                .As<IHistoryEventsCreator>();

            services.AddSingletonAs<SchemaHistoryEventsCreator>()
                .As<IHistoryEventsCreator>();

            services.AddSingletonAs<EdmModelBuilder>()
                .AsSelf();

            services.AddSingletonAs<GrainTagService>()
                .As<ITagService>();

            services.AddSingletonAs<FileTypeTagGenerator>()
                .As<ITagGenerator<CreateAsset>>();

            services.AddSingletonAs<ImageTagGenerator>()
                .As<ITagGenerator<CreateAsset>>();

            services.AddSingletonAs<InMemoryCommandBus>()
                .As<ICommandBus>();

            services.AddSingletonAs<ReadonlyCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<ETagCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<EnrichWithTimestampCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<EnrichWithActorCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<EnrichWithAppIdCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<EnrichWithSchemaIdCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<AssetCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<GrainCommandMiddleware<AppCommand, IAppGrain>>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<GrainCommandMiddleware<ContentCommand, IContentGrain>>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<GrainCommandMiddleware<SchemaCommand, ISchemaGrain>>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<GrainCommandMiddleware<RuleCommand, IRuleGrain>>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<AppsByNameIndexCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<AppsByUserIndexCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<RulesByAppIndexCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<SchemasByAppIndexCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<SingletonCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<CreateBlogCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<CreateProfileCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<JintScriptEngine>()
                .As<IScriptEngine>();

            services.AddSingleton<Func<IGrainCallContext, string>>(DomainObjectGrainFormatter.Format);

            services.AddSingleton(c =>
            {
                var uiOptions = c.GetRequiredService<IOptions<MyUIOptions>>();

                var result = new InitialPatterns();

                foreach (var pattern in uiOptions.Value.RegexSuggestions)
                {
                    if (!string.IsNullOrWhiteSpace(pattern.Key) &&
                        !string.IsNullOrWhiteSpace(pattern.Value))
                    {
                        result[Guid.NewGuid()] = new AppPattern(pattern.Key, pattern.Value);
                    }
                }

                return result;
            });
        }

        public static void AddMyMigrationServices(this IServiceCollection services)
        {
            services.AddSingletonAs<Migrator>()
                .AsSelf();

            services.AddTransientAs<MigrationPath>()
                .As<IMigrationPath>();

            services.AddTransientAs<AddPatterns>()
                .As<IMigration>();

            services.AddTransientAs<ConvertEventStore>()
                .As<IMigration>();

            services.AddTransientAs<ConvertEventStoreAppId>()
                .As<IMigration>();

            services.AddTransientAs<PopulateGrainIndexes>()
                .As<IMigration>();

            services.AddTransientAs<RebuildContents>()
                .As<IMigration>();

            services.AddTransientAs<RebuildSnapshots>()
                .As<IMigration>();

            services.AddTransientAs<RebuildAssets>()
                .As<IMigration>();

            services.AddTransientAs<StartEventConsumers>()
                .As<IMigration>();

            services.AddTransientAs<StopEventConsumers>()
                .As<IMigration>();

            services.AddTransientAs<Rebuilder>()
                .AsSelf();
        }
    }
}
