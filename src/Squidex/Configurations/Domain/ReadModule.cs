// ==========================================================================
//  ReadModule.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using PinkParrot.Infrastructure.CQRS.Events;
using PinkParrot.Read.Apps.Services;
using PinkParrot.Read.Apps.Services.Implementations;
using PinkParrot.Read.Schemas.Services;
using PinkParrot.Read.Schemas.Services.Implementations;

namespace PinkParrot.Configurations.Domain
{
    public sealed class ReadModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CachingAppProvider>()
                .As<IAppProvider>()
                .SingleInstance();

            builder.RegisterType<CachingSchemaProvider>()
                .As<ISchemaProvider>()
                .As<ILiveEventConsumer>()
                .SingleInstance();
        }
    }
}
