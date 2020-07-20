// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Apps.Invitation;
using Squidex.Domain.Apps.Entities.Apps.Templates;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Comments;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.Rules.UsageTracking;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Web.CommandMiddlewares;

namespace Squidex.Config.Domain
{
    public static class CommandsServices
    {
        public static void AddSquidexCommands(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<ReadonlyOptions>(
                config.GetSection("mode"));

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

            services.AddSingletonAs<CustomCommandMiddlewareRunner>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<InviteUserCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<AppsIndex>()
                .As<ICommandMiddleware>().As<IAppsIndex>();

            services.AddSingletonAs<RulesIndex>()
                .As<ICommandMiddleware>().As<IRulesIndex>();

            services.AddSingletonAs<SchemasIndex>()
                .As<ICommandMiddleware>().As<ISchemasIndex>();

            services.AddSingletonAs<AppCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<AssetCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<BulkUpdateCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<ContentCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<RuleCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<CommentsCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<GrainCommandMiddleware<AssetFolderCommand, IAssetFolderGrain>>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<GrainCommandMiddleware<SchemaCommand, ISchemaGrain>>()
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

            services.AddSingleton(typeof(IEventEnricher<>), typeof(DefaultEventEnricher<>));
        }
    }
}