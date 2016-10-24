// ==========================================================================
//  WebpackServices.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using PinkParrot.Pipeline;

namespace PinkParrot.Configurations.Web
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
