// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Config.Domain;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure.Caching;
using Squidex.Pipeline.Plugins;
using Squidex.Pipeline.Robots;
using Squidex.Web;
using Squidex.Web.Pipeline;

namespace Squidex.Config.Web
{
    public static class WebServices
    {
        public static void AddSquidexMvcWithPlugins(this IServiceCollection services, IConfiguration config)
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

            services.AddSingletonAs<UsageMiddleware>()
                .AsSelf();

            services.AddSingletonAs<RequestExceptionMiddleware>()
                .AsSelf();

            services.AddSingletonAs<RequestLogPerformanceMiddleware>()
                .AsSelf();

            services.AddSingletonAs<CachingManager>()
                .As<IRequestCache>();

            services.AddSingletonAs<ContextProvider>()
                .As<IContextProvider>();

            services.AddSingletonAs<HttpContextAccessor>()
                .As<IHttpContextAccessor>();

            services.AddSingletonAs<ActionContextAccessor>()
                .As<IActionContextAccessor>();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddMvc(options =>
            {
                options.Filters.Add<CachingFilter>();
                options.Filters.Add<DeferredActionFilter>();
                options.Filters.Add<AppResolver>();
                options.Filters.Add<MeasureResultFilter>();
            })
            .AddSquidexPlugins(config)
            .AddSquidexSerializers();
        }
    }
}
