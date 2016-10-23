// ==========================================================================
//  WebDependencies.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using PinkParrot.Configurations.Domain;

namespace PinkParrot.Configurations.Web
{
    public static class WebDependencies
    {
        public static void AddMyMvc(this IServiceCollection services)
        {
            services.AddMvc().AddMySerializers();
        }
    }
}
