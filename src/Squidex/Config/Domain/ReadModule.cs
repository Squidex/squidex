// ==========================================================================
//  ReadModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using Microsoft.Extensions.Configuration;
using Squidex.Read.Apps;
using Squidex.Read.Apps.Services;
using Squidex.Read.Apps.Services.Implementations;
using Squidex.Read.Contents;
using Squidex.Read.Contents.Builders;
using Squidex.Read.History;
using Squidex.Read.Schemas;
using Squidex.Read.Schemas.Services;
using Squidex.Read.Schemas.Services.Implementations;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Squidex.Config.Domain
{
    public sealed class ReadModule : Module
    {
        private IConfiguration Configuration { get; }

        public ReadModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CachingAppProvider>()
                .As<IAppProvider>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<CachingSchemaProvider>()
                .As<ISchemaProvider>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<AppHistoryEventsCreator>()
                .As<IHistoryEventsCreator>()
                .SingleInstance();

            builder.RegisterType<ContentHistoryEventsCreator>()
                .As<IHistoryEventsCreator>()
                .SingleInstance();

            builder.RegisterType<SchemaHistoryEventsCreator>()
                .As<IHistoryEventsCreator>()
                .SingleInstance();

            builder.RegisterType<EdmModelBuilder>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
