// ==========================================================================
//  WriteServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Write.Apps;
using Squidex.Domain.Apps.Write.Assets;
using Squidex.Domain.Apps.Write.Contents;
using Squidex.Domain.Apps.Write.Rules;
using Squidex.Domain.Apps.Write.Schemas;
using Squidex.Domain.Users;
using Squidex.Infrastructure.Commands;
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

            services.AddSingletonAs<ContentVersionLoader>()
                .As<IContentVersionLoader>();

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

            services.AddSingletonAs<DomainObjectFactoryFunction<AppDomainObject>>(c =>
            {
                var patterns = c.GetRequiredService<IOptions<List<Squidex.Domain.Apps.Core.Apps.AppPattern>>>();

                return id => new AppDomainObject(patterns, id, -1);
            });

            services.AddSingletonAs<DomainObjectFactoryFunction<RuleDomainObject>>(c => (id => new RuleDomainObject(id, -1)));
            services.AddSingletonAs<DomainObjectFactoryFunction<AssetDomainObject>>(c => (id => new AssetDomainObject(id, -1)));
            services.AddSingletonAs<DomainObjectFactoryFunction<ContentDomainObject>>(c => (id => new ContentDomainObject(id, -1)));

            services.AddSingletonAs<DomainObjectFactoryFunction<SchemaDomainObject>>(c =>
                {
                    var fieldRegistry = c.GetRequiredService<FieldRegistry>();

                    return id => new SchemaDomainObject(id, -1, fieldRegistry);
                });
        }
    }
}
