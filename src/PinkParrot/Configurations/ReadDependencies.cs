// ==========================================================================
//  ReadDependencies.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using PinkParrot.Read.Services;
using PinkParrot.Read.Services.Implementations;

namespace PinkParrot.Configurations
{
    public sealed class ReadDependencies : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SchemaProvider>()
                .As<ISchemaProvider>()
                .SingleInstance();
        }
    }
}
