// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Diagnostics;

namespace Squidex.Config.Domain;

public static class HealthCheckServices
{
    public static void AddSquidexHealthChecks(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<GCHealthCheckOptions>(config,
            "diagnostics:gc");

        services.AddHealthChecks()
            .AddCheck<GCHealthCheck>("GC", tags: new[] { "node" });
    }
}
