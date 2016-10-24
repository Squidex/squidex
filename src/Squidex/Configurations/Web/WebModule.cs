// ==========================================================================
//  WebModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using Squidex.Pipeline;

namespace Squidex.Configurations.Web
{
    public class WebModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppFilterAttribute>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
