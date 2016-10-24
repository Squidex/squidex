// ==========================================================================
//  WebpackServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Pipeline;

namespace Squidex.Configurations.Web
{
    public static class WebpackServices
    {
        public static IServiceCollection AddWebpackBuilder(this IServiceCollection services)
        {
            services.AddSingleton<WebpackRunner>();

            return services;
        }
    }
}
