// ==========================================================================
//  WriteServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Migrate_01;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Users;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Migrations;
using Squidex.Pipeline.CommandMiddlewares;

namespace Squidex.Config.Domain
{
    public static class WriteServices
    {
        public static void AddMyWriteServices(this IServiceCollection services)
        {
            services.AddSingletonAs<NoopUserEvents>()
                .As<IUserEvents>();

            services.AddSingletonAs<JintScriptEngine>()
                .As<IScriptEngine>();

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

            services.AddSingletonAs<AppCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<AssetCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<ContentCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<SchemaCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingletonAs<RuleCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddTransientAs<Migration01>()
                .As<IMigration>();

            services.AddTransientAs<AppDomainObject>()
                .AsSelf();

            services.AddTransientAs<AssetDomainObject>()
                .AsSelf();

            services.AddTransientAs<ContentDomainObject>()
                .AsSelf();

            services.AddTransientAs<RuleDomainObject>()
                .AsSelf();

            services.AddTransientAs<SchemaDomainObject>()
                .AsSelf();
        }
    }
}
