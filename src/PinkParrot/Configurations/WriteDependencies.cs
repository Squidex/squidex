// ==========================================================================
//  WriteDependencies.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Write.Schema;

namespace PinkParrot.Configurations
{
    public class WriteDependencies : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ModelSchemaCommandHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<ModelSchemaDomainObject>()
                .InstancePerDependency();
        }
    }
}
