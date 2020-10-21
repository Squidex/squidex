// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Squidex.Config.Domain;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Translations;
using Squidex.Pipeline.Plugins;
using Squidex.Pipeline.Robots;
using Squidex.Shared;
using Squidex.Web;
using Squidex.Web.Pipeline;
using Squidex.Web.Services;

namespace Squidex.Config.Web
{
    public static class WebServices
    {
        public static void AddSquidexMvcWithPlugins(this IServiceCollection services, IConfiguration config)
        {
            var translator = new ResourcesLocalizer(Texts.ResourceManager);

            T.Setup(translator);

            services.AddSingletonAs(c => new ExposedValues(c.GetRequiredService<IOptions<ExposedConfiguration>>().Value, config, typeof(WebServices).Assembly))
                .AsSelf();

            services.AddSingletonAs<FileCallbackResultExecutor>()
                .AsSelf();

            services.AddSingletonAs<ApiCostsFilter>()
                .AsSelf();

            services.AddSingletonAs<AppResolver>()
                .AsSelf();

            services.AddSingletonAs<SchemaResolver>()
                .AsSelf();

            services.AddSingletonAs<RobotsTxtMiddleware>()
                .AsSelf();

            services.AddSingletonAs<LocalCacheMiddleware>()
                .AsSelf();

            services.AddSingletonAs<UsageMiddleware>()
                .AsSelf();

            services.AddSingletonAs<RequestExceptionMiddleware>()
                .AsSelf();

            services.AddSingletonAs<RequestLogPerformanceMiddleware>()
                .AsSelf();

            services.AddSingletonAs(c => translator)
                .As<ILocalizer>();

            services.AddSingletonAs<StringLocalizer>()
                .As<IStringLocalizer>().As<IStringLocalizerFactory>();

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

            services.AddLocalization();

            services.AddMvc(options =>
            {
                options.Filters.Add<CachingFilter>();
                options.Filters.Add<DeferredActionFilter>();
                options.Filters.Add<AppResolver>();
                options.Filters.Add<SchemaResolver>();
                options.Filters.Add<MeasureResultFilter>();
            })
            .AddDataAnnotationsLocalization()
            .AddRazorRuntimeCompilation()
            .AddSquidexPlugins(config)
            .AddSquidexSerializers();

            var urlsOptions = config.GetSection("urls").Get<UrlsOptions>();

            var host = urlsOptions.BuildHost();

            if (urlsOptions.EnforceHost)
            {
                services.AddHostFiltering(options =>
                {
                    options.AllowEmptyHosts = true;
                    options.AllowedHosts.Add(host.Host);

                    options.IncludeFailureMessage = false;
                });
            }

            if (urlsOptions.EnforceHTTPS && !string.Equals(host.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                services.AddHttpsRedirection(options =>
                {
                    options.HttpsPort = urlsOptions.HttpsPort;
                });
            }
        }
    }
}
