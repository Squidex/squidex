// ==========================================================================
//  WriteServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Write.Apps;
using Squidex.Domain.Apps.Write.Assets;
using Squidex.Domain.Apps.Write.Contents;
using Squidex.Domain.Apps.Write.Rules;
using Squidex.Domain.Apps.Write.Schemas;
using Squidex.Domain.Users;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Pipeline.CommandMiddlewares;

namespace Squidex.Config.Domain
{
    public static class WriteServices
    {
        public static void AddMyWriteServices(this IServiceCollection services)
        {
            services.AddSingleton<NoopUserEvents>()
                .As<IUserEvents>();

            services.AddSingleton<JintScriptEngine>()
                .As<IScriptEngine>();

            services.AddSingleton<ContentVersionLoader>()
                .As<IContentVersionLoader>();

            services.AddSingleton<ETagCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingleton<EnrichWithTimestampCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingleton<EnrichWithActorCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingleton<EnrichWithAppIdCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingleton<EnrichWithSchemaIdCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingleton<AppCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingleton<AssetCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingleton<ContentCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingleton<SchemaCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingleton<RuleCommandMiddleware>()
                .As<ICommandMiddleware>();

            services.AddSingleton<DomainObjectFactoryFunction<AppDomainObject>>(c => (id => new AppDomainObject(id, -1)));
            services.AddSingleton<DomainObjectFactoryFunction<RuleDomainObject>>(c => (id => new RuleDomainObject(id, -1)));
            services.AddSingleton<DomainObjectFactoryFunction<AssetDomainObject>>(c => (id => new AssetDomainObject(id, -1)));
            services.AddSingleton<DomainObjectFactoryFunction<ContentDomainObject>>(c => (id => new ContentDomainObject(id, -1)));

            services.AddSingleton<DomainObjectFactoryFunction<SchemaDomainObject>>(c =>
                {
                    var fieldRegistry = c.GetRequiredService<FieldRegistry>();

                    return id => new SchemaDomainObject(id, -1, fieldRegistry);
                });
        }
    }
}
