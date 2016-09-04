// ==========================================================================
//  WriteDependencies.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Pipeline.CommandHandlers;
using PinkParrot.Write.Schema;

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

            builder.RegisterType<ModelSchemaCommandHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<ModelSchemaDomainObject>()
                .AsSelf()
                .InstancePerDependency();
        }
    }
}
