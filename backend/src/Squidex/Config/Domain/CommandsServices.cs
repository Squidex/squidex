// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Domain.Apps.Entities.Apps.Templates;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Domain.Apps.Entities.Comments.DomainObject;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Domain.Apps.Entities.Invitation;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.Rules.UsageTracking;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Domain.Apps.Entities.Teams.Commands;
using Squidex.Domain.Apps.Entities.Teams.DomainObject;
using Squidex.Domain.Apps.Entities.Teams.Indexes;
using Squidex.Infrastructure.Commands;
using Squidex.Web.CommandMiddlewares;

namespace Squidex.Config.Domain;

public static class CommandsServices
{
    public static void AddSquidexCommands(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<ReadonlyOptions>(config,
            "mode");

        services.Configure<RestrictAppsOptions>(config,
            "usage");

        services.Configure<DomainObjectCacheOptions>(config,
            "caching:domainObjects");

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

        services.AddSingletonAs<EnrichWithTeamIdCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<EnrichWithSchemaIdCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<EnrichWithContentIdCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<CustomCommandMiddlewareRunner>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<TemplateCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<AlwaysCreateClientCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<RestrictAppsCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<InviteUserCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<AppsIndex>()
            .As<ICommandMiddleware>().As<IAppsIndex>();

        services.AddSingletonAs<RulesIndex>()
            .As<IRulesIndex>();

        services.AddSingletonAs<SchemasIndex>()
            .As<ICommandMiddleware>().As<ISchemasIndex>();

        services.AddSingletonAs<TeamsIndex>()
            .As<ITeamsIndex>();

        services.AddSingletonAs<AppCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<AssetsBulkUpdateCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<AssetCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<CommentsCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<ContentsBulkUpdateCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<ContentCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<RuleCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<AggregateCommandMiddleware<AssetFolderCommandBase, AssetFolderDomainObject>>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<AggregateCommandMiddleware<SchemaCommandBase, SchemaDomainObject>>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<AggregateCommandMiddleware<TeamCommandBase, TeamDomainObject>>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<SingletonCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<UsageTrackerCommandMiddleware>()
            .As<ICommandMiddleware>();

        services.AddSingletonAs<DefaultDomainObjectFactory>()
            .As<IDomainObjectFactory>();

        services.AddSingletonAs<DefaultDomainObjectCache>()
            .As<IDomainObjectCache>();
    }
}
