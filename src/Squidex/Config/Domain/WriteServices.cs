// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Migrate_01;
using Migrate_01.Migrations;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Templates;
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

            services.AddSingletonAs<CreateBlogCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddTransientAs<MigrationPath>()
                .As<IMigrationPath>();

            services.AddTransientAs<ConvertEventStore>()
                .As<IMigration>();

            services.AddTransientAs<AddPatterns>()
                .As<IMigration>();

            services.AddTransientAs<RebuildContents>()
                .As<IMigration>();

            services.AddTransientAs<RebuildSnapshots>()
                .As<IMigration>();

            services.AddTransientAs<RebuildAssets>()
                .As<IMigration>();

            services.AddTransientAs<Rebuilder>()
                .AsSelf();

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

            services.AddSingleton<InitialPatterns>(c =>
            {
                var config = c.GetRequiredService<IOptions<MyUIOptions>>();

                var result = new InitialPatterns();

                foreach (var pattern in config.Value.RegexSuggestions)
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
    }
}
