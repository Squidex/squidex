// ==========================================================================
//  WebDependencies.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Configurations.Domain;

namespace Squidex.Configurations.Web
{
    public static class WebDependencies
    {
        public static void AddMyMvc(this IServiceCollection services)
        {
            services.AddMvc().AddMySerializers();
        }
    }
}
