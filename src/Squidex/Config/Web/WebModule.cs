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
        public IConfiguration Configuration { get; }

        public WebModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppFilterAttribute>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
