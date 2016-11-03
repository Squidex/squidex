// ==========================================================================
//  WriteModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Pipeline.CommandHandlers;
using Squidex.Write.Apps;
using Squidex.Write.Schemas;

namespace Squidex.Configurations.Domain
{
    public class WriteModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EnrichWithSchemaAggregateIdHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<EnrichWithAppIdHandler>()
                .As<ICommandHandler>()
                .SingleInstance();

            builder.RegisterType<EnrichWithSubjectHandler>()
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
