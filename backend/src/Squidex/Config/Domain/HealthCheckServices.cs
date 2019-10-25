﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Apps.Diagnostics;
using Squidex.Infrastructure.Diagnostics;

namespace Squidex.Config.Domain
{
    public static class HealthCheckServices
    {
        public static void AddSquidexHealthChecks(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<GCHealthCheckOptions>(
                config.GetSection("healthz:gc"));

            services.AddHealthChecks()
                .AddCheck<GCHealthCheck>("GC", tags: new[] { "node" })
                .AddCheck<OrleansHealthCheck>("Orleans", tags: new[] { "cluster" })
                .AddCheck<OrleansAppsHealthCheck>("Orleans App", tags: new[] { "cluster" });
        }
    }
}
