// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL;
using GraphQL.DataLoader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Migrate_01;
using Migrate_01.Migrations;
using Orleans;
using Squidex.Areas.Api.Controllers.UI;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Apps.Invitation;
using Squidex.Domain.Apps.Entities.Apps.Templates;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Comments;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Edm;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.History.Notifications;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.Rules.UsageTracking;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Domain.Apps.Entities.Tags;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Email;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Orleans;
using Squidex.Web;
using Squidex.Web.CommandMiddlewares;
using Squidex.Web.Services;

namespace Squidex.Config.Domain
{
    public static class EntitiesServices
    {
        public static void AddMyEntitiesServices(this IServiceCollection services, IConfiguration config)
        {
            var exposeSourceUrl = config.GetOptionalValue("assetStore:exposeSourceUrl", true);

            services.AddSingletonAs(c => new UrlGenerator(
                    c.GetRequiredService<IOptions<UrlsOptions>>(),
                    c.GetRequiredService<IAssetStore>(),
                    exposeSourceUrl))
                .As<IGraphQLUrlGenerator>().As<IRuleUrlGenerator>().As<IAssetUrlGenerator>().As<IEmailUrlGenerator>();

            services.AddSingletonAs<HistoryService>()
                .As<IEventConsumer>().As<IHistoryService>();

            services.AddSingletonAs<AssetUsageTracker>()
                .As<IEventConsumer>().As<IAssetUsageTracker>();

            services.AddSingletonAs(x => new FuncDependencyResolver(t => x.GetRequiredService(t)))
                .As<IDependencyResolver>();

            services.AddSingletonAs<DataLoaderContextAccessor>()
                .As<IDataLoaderContextAccessor>();

            services.AddSingletonAs<DataLoaderDocumentListener>()
                .AsSelf();

            services.AddSingletonAs<CachingGraphQLService>()
                .As<IGraphQLService>();

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

            services.AddSingletonAs<RolePermissionsProvider>()
                .AsSelf();

            services.AddSingletonAs<EdmModelBuilder>()
                .AsSelf();

            services.AddSingletonAs<GrainTagService>()
                .As<ITagService>();

            services.AddSingletonAs<GrainTextIndexer>()
                .As<ITextIndexer>().As<IEventConsumer>();

            services.AddSingletonAs<FileTypeTagGenerator>()
                .As<ITagGenerator<CreateAsset>>();

            services.AddSingletonAs<ImageTagGenerator>()
                .As<ITagGenerator<CreateAsset>>();

            services.AddSingletonAs<JintScriptEngine>()
                .AsOptional<IScriptEngine>();

            services.AddSingletonAs<GrainBootstrap<IContentSchedulerGrain>>()
                .AsSelf();

            services.AddSingletonAs<GrainBootstrap<IRuleDequeuerGrain>>()
                .AsSelf();

            services.AddCommandPipeline();
            services.AddBackupHandlers();

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

            var emailOptions = config.GetSection("email:smtp").Get<SmptOptions>();

            if (emailOptions.IsConfigured())
            {
                services.AddSingleton(Options.Create(emailOptions));

                services.Configure<NotificationEmailTextOptions>(
                    config.GetSection("email:notifications"));

                services.AddSingletonAs<SmtpEmailSender>()
                    .As<IEmailSender>();

                services.AddSingletonAs<NotificationEmailSender>()
                    .AsOptional<INotificationEmailSender>();
            }
            else
            {
                services.AddSingletonAs<NoopNotificationEmailSender>()
                    .AsOptional<INotificationEmailSender>();
            }

            services.AddSingletonAs<NotificationEmailEventConsumer>()
                .As<IEventConsumer>();
        }

        private static void AddCommandPipeline(this IServiceCollection services)
        {
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

            services.AddSingletonAs<InviteUserCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<AssetCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<AppsByNameIndexCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<GrainCommandMiddleware<AppCommand, IAppGrain>>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<GrainCommandMiddleware<CommentsCommand, ICommentGrain>>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<GrainCommandMiddleware<ContentCommand, IContentGrain>>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<GrainCommandMiddleware<SchemaCommand, ISchemaGrain>>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<GrainCommandMiddleware<RuleCommand, IRuleGrain>>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<AppsByUserIndexCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<RulesByAppIndexCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<SchemasByAppIndexCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<SingletonCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<AlwaysCreateClientCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<CreateBlogCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<CreateIdentityCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<CreateProfileCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<UsageTrackerCommandMiddleware>()
                .As<ICommandMiddleware>();
        }

        private static void AddBackupHandlers(this IServiceCollection services)
        {
            services.AddTransientAs<BackupApps>()
                .As<BackupHandler>();

            services.AddTransientAs<BackupAssets>()
                .As<BackupHandler>();

            services.AddTransientAs<BackupContents>()
                .As<BackupHandler>();

            services.AddTransientAs<BackupRules>()
                .As<BackupHandler>();

            services.AddTransientAs<BackupSchemas>()
                .As<BackupHandler>();
        }

        public static void AddMyMigrationServices(this IServiceCollection services)
        {
            services.AddSingletonAs<Migrator>()
                .AsSelf();

            services.AddTransientAs<Rebuilder>()
                .AsSelf();

            services.AddTransientAs<RebuildRunner>()
                .AsSelf();

            services.AddTransientAs<MigrationPath>()
                .As<IMigrationPath>();

            services.AddTransientAs<AddPatterns>()
                .As<IMigration>();

            services.AddTransientAs<ConvertEventStore>()
                .As<IMigration>();

            services.AddTransientAs<ConvertEventStoreAppId>()
                .As<IMigration>();

            services.AddTransientAs<ClearSchemas>()
                .As<IMigration>();

            services.AddTransientAs<CreateAssetSlugs>()
                .As<IMigration>();

            services.AddTransientAs<PopulateGrainIndexes>()
                .As<IMigration>();

            services.AddTransientAs<RebuildContents>()
                .As<IMigration>();

            services.AddTransientAs<RebuildSnapshots>()
                .As<IMigration>();

            services.AddTransientAs<RebuildApps>()
                .As<IMigration>();

            services.AddTransientAs<RebuildAssets>()
                .As<IMigration>();

            services.AddTransientAs<StartEventConsumers>()
                .As<IMigration>();

            services.AddTransientAs<StopEventConsumers>()
                .As<IMigration>();
        }
    }
}
