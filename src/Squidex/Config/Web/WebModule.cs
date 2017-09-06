// ==========================================================================
//  WebModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using Microsoft.Extensions.Configuration;
using Squidex.Pipeline;

namespace Squidex.Config.Web
{
    public class WebModule : Module
    {
        private IConfiguration Configuration { get; }

        public WebModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppApiFilter>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<FileCallbackResultExecutor>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
