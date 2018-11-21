// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Diagnostics;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Shared.Users;

#pragma warning disable RECS0092 // Convert field to readonly

namespace Squidex.Config.Domain
{
    public static class InfrastructureServices
    {
        public static void AddMyInfrastructureServices(this IServiceCollection services)
        {
            services.AddSingletonAs(SystemClock.Instance)
                .As<IClock>();

            services.AddSingletonAs<BackgroundUsageTracker>()
                .AsSelf();

            services.AddSingletonAs(c => new CachingUsageTracker(c.GetRequiredService<BackgroundUsageTracker>(), c.GetRequiredService<IMemoryCache>()))
                .As<IUsageTracker>();

            services.AddSingletonAs<AsyncLocalCache>()
                .As<ILocalCache>();

            services.AddSingletonAs<GCHealthCheck>()
                .As<IHealthCheck>();

            services.AddSingletonAs<HttpContextAccessor>()
                .As<IHttpContextAccessor>();

            services.AddSingletonAs<ActionContextAccessor>()
                .As<IActionContextAccessor>();

            services.AddSingletonAs<DefaultUserResolver>()
                .As<IUserResolver>();

            services.AddSingletonAs<DefaultXmlRepository>()
                .As<IXmlRepository>();

            services.AddSingletonAs<AssetUserPictureStore>()
                .As<IUserPictureStore>();

            services.AddTransient(typeof(Lazy<>), typeof(Lazier<>));
        }
    }
}
