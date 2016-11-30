// ==========================================================================
//  WriteModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using Autofac;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Pipeline.CommandHandlers;
using Squidex.Write;
using Squidex.Write.Apps;
using Squidex.Write.Schemas;

namespace Squidex.Config.Domain
{
    public class WriteModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EnrichWithAppIdHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<EnrichWithUserHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<EnrichWithTimestampHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<EnrichWithSchemaAggregateIdHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<EnrichWithAppIdProcessor>()
                .As<IEventProcessor>()
                .SingleInstance();

            builder.RegisterType<EnrichWithAggregateIdProcessor>()
                .As<IEventProcessor>()
                .SingleInstance();

            builder.RegisterType<EnrichWithUserProcessor>()
                .As<IEventProcessor>()
                .SingleInstance();

            builder.RegisterType<AppCommandHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<ClientKeyGenerator>()
                .AsSelf()
                .InstancePerDependency();

            builder.RegisterType<AppDomainObject>()
                .AsSelf()
                .InstancePerDependency();

            builder.RegisterType<SchemaCommandHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<SchemaDomainObject>()
                .AsSelf()
                .InstancePerDependency();
        }
    }
}
