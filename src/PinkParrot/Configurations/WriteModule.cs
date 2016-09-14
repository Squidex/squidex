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
using PinkParrot.Write.Schemas;

namespace PinkParrot.Configurations
{
    public class WriteModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EnrichWithAggregateIdHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<EnrichWithTenantIdHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<SchemaCommandHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<SchemaDomainObject>()
                .AsSelf()
                .InstancePerDependency();
        }
    }
}
