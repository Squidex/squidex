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
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Pipeline.CommandHandlers;
using Squidex.Write;
using Squidex.Write.Apps;
using Squidex.Write.Contents;
using Squidex.Write.Schemas;

namespace Squidex.Config.Domain
{
    public class WriteModule : Module
    {
        public IConfiguration Configuration { get; }

        public WriteModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
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


            builder.RegisterType<EnrichWithAppIdProcessor>()
                .As<IEventProcessor>()
                .SingleInstance();

            builder.RegisterType<EnrichWithSchemaIdProcessor>()
                .As<IEventProcessor>()
                .SingleInstance();

            builder.RegisterType<EnrichWithAggregateIdProcessor>()
                .As<IEventProcessor>()
                .SingleInstance();

            builder.RegisterType<EnrichWithActorProcessor>()
                .As<IEventProcessor>()
                .SingleInstance();


            builder.RegisterType<ClientKeyGenerator>()
                .AsSelf()
                .InstancePerDependency();


            builder.RegisterType<FieldRegistry>()
                .AsSelf()
                .InstancePerDependency();


            builder.RegisterType<AppCommandHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<ContentCommandHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<SchemaCommandHandler>()
                .As<ICommandHandler>()
                .SingleInstance();


            builder.Register<DomainObjectFactoryFunction<AppDomainObject>>(s => (id => new AppDomainObject(id, 0)))
                .AsSelf()
                .InstancePerDependency();

            builder.Register<DomainObjectFactoryFunction<ContentDomainObject>>(s => (id => new ContentDomainObject(id, 0)))
                .AsSelf()
                .InstancePerDependency();

            builder.Register<DomainObjectFactoryFunction<SchemaDomainObject>>(s => (id => new SchemaDomainObject(id, 0, s.Resolve<FieldRegistry>())))
                .AsSelf()
                .InstancePerDependency();
        }
    }
}
