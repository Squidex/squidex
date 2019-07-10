// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Config.Domain;
using Squidex.Domain.Apps.Entities;
using Squidex.Pipeline.Plugins;
using Squidex.Pipeline.Robots;
using Squidex.Web;
using Squidex.Web.Pipeline;

namespace Squidex.Config.Web
{
    public static class WebServices
    {
        public static void AddMyMvcWithPlugins(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingletonAs(c => new ExposedValues(c.GetRequiredService<IOptions<ExposedConfiguration>>().Value, config, typeof(WebServices).Assembly))
                .AsSelf();

            services.AddSingletonAs<FileCallbackResultExecutor>()
                .AsSelf();

            services.AddSingletonAs<ApiCostsFilter>()
                .AsSelf();

            services.AddSingletonAs<AppResolver>()
                .AsSelf();

            services.AddSingletonAs<RobotsTxtMiddleware>()
                .AsSelf();

            services.AddSingletonAs<EnforceHttpsMiddleware>()
                .AsSelf();

            services.AddSingletonAs<LocalCacheMiddleware>()
                .AsSelf();

            services.AddSingletonAs<RequestLogPerformanceMiddleware>()
                .AsSelf();

            services.AddSingletonAs<ContextProvider>()
                .As<IContextProvider>();

            services.AddSingletonAs<ApiPermissionUnifier>()
                .AsOptional<IClaimsTransformation>();

            services.AddMvc(options =>
            {
                options.Filters.Add<ETagFilter>();
                options.Filters.Add<DeferredActionFilter>();
                options.Filters.Add<AppResolver>();
                options.Filters.Add<MeasureResultFilter>();
            })
            .AddMyPlugins(config)
            .AddMySerializers();

            services.AddCors();
            services.AddRouting();
        }
    }
}
