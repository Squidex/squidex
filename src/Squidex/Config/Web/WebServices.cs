// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Config.Domain;
using Squidex.Pipeline;
using Squidex.Pipeline.Robots;

namespace Squidex.Config.Web
{
    public static class WebServices
    {
        public static void AddMyMvc(this IServiceCollection services)
        {
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

            services.AddSingletonAs<ApiPermissionUnifier>()
                .As<IClaimsTransformation>();

            services.AddMvc(options =>
            {
                options.Filters.Add<ETagFilter>();
                options.Filters.Add<AppResolver>();
                options.Filters.Add<MeasureResultFilter>();
            }).AddMySerializers();

            services.AddCors();
            services.AddRouting();
        }
    }
}
