// ==========================================================================
//  ReadModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Read.Apps;
using Squidex.Read.Apps.Services;
using Squidex.Read.Apps.Services.Implementations;
using Squidex.Read.History;
using Squidex.Read.Schemas;
using Squidex.Read.Schemas.Services;
using Squidex.Read.Schemas.Services.Implementations;

namespace Squidex.Config.Domain
{
    public sealed class ReadModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CachingAppProvider>()
                .As<IAppProvider>()
                .As<ICatchEventConsumer>()
                .SingleInstance();

            builder.RegisterType<CachingSchemaProvider>()
                .As<ISchemaProvider>()
                .As<ICatchEventConsumer>()
                .SingleInstance();

            builder.RegisterType<AppHistoryEventsCreator>()
                .As<IHistoryEventsCreator>()
                .SingleInstance();

            builder.RegisterType<SchemaHistoryEventsCreator>()
                .As<IHistoryEventsCreator>()
                .SingleInstance();
        }
    }
}
