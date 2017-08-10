// ==========================================================================
//  WriteModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using Microsoft.Extensions.Configuration;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Write.Apps;
using Squidex.Domain.Apps.Write.Assets;
using Squidex.Domain.Apps.Write.Contents;
using Squidex.Domain.Apps.Write.Schemas;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Pipeline.CommandHandlers;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Squidex.Config.Domain
{
    public class WriteModule : Module
    {
        private IConfiguration Configuration { get; }

        public WriteModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ETagCommandMiddleware>()
                .As<ICommandMiddleware>()
                .SingleInstance();

            builder.RegisterType<EnrichWithTimestampHandler>()
                .As<ICommandMiddleware>()
                .SingleInstance();

            builder.RegisterType<EnrichWithActorCommandMiddleware>()
                .As<ICommandMiddleware>()
                .SingleInstance();

            builder.RegisterType<EnrichWithAppIdCommandMiddleware>()
                .As<ICommandMiddleware>()
                .SingleInstance();

            builder.RegisterType<EnrichWithSchemaIdCommandMiddleware>()
                .As<ICommandMiddleware>()
                .SingleInstance();

            builder.RegisterType<FieldRegistry>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<AppCommandMiddleware>()
                .As<ICommandMiddleware>()
                .SingleInstance();

            builder.RegisterType<AssetCommandMiddleware>()
                .As<ICommandMiddleware>()
                .SingleInstance();

            builder.RegisterType<ContentCommandMiddleware>()
                .As<ICommandMiddleware>()
                .SingleInstance();

            builder.RegisterType<SchemaCommandMiddleware>()
                .As<ICommandMiddleware>()
                .SingleInstance();

            builder.RegisterType<ETagCommandMiddleware>()
                .As<ICommandMiddleware>()
                .SingleInstance();

            builder.Register<DomainObjectFactoryFunction<AppDomainObject>>(c => (id => new AppDomainObject(id, -1)))
                .AsSelf()
                .SingleInstance();

            builder.Register<DomainObjectFactoryFunction<AssetDomainObject>>(c => (id => new AssetDomainObject(id, -1)))
                .AsSelf()
                .SingleInstance();

            builder.Register<DomainObjectFactoryFunction<ContentDomainObject>>(c => (id => new ContentDomainObject(id, -1)))
                .AsSelf()
                .SingleInstance();

            builder.Register<DomainObjectFactoryFunction<SchemaDomainObject>>(c =>
                {
                    var fieldRegistry = c.Resolve<FieldRegistry>();

                    return (id => new SchemaDomainObject(id, -1, fieldRegistry));
                })
                .AsSelf()
                .SingleInstance();
        }
    }
}
