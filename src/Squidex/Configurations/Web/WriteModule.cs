// ==========================================================================
//  WriteModule.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Pipeline.CommandHandlers;
using PinkParrot.Write.Apps;
using PinkParrot.Write.Schemas;

namespace PinkParrot.Configurations.Web
{
    public class WriteModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EnrichWithAggregateIdHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<EnrichWithAppIdHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<AppCommandHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

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
