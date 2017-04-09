// ==========================================================================
//  WriteModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using Microsoft.Extensions.Configuration;
using Squidex.Core.Schemas;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Pipeline.CommandHandlers;
using Squidex.Write.Apps;
using Squidex.Write.Assets;
using Squidex.Write.Contents;
using Squidex.Write.Schemas;

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
            builder.RegisterType<EnrichWithExpectedVersionHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<EnrichWithTimestampHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<EnrichWithActorHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<EnrichWithAppIdHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<EnrichWithSchemaIdHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<ClientKeyGenerator>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<FieldRegistry>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<AppCommandHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<AssetCommandHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<ContentCommandHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<SchemaCommandHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<SetVersionAsETagHandler>()
                .As<ICommandHandler>()
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
