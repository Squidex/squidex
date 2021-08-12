// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Squidex.Config.Domain
{
    public static class TelemetryServices
    {
        public static void AddSquidexTelemetry(this IServiceCollection services)
        {
            services.AddOpenTelemetryTracing(builder =>
            {
                builder.SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService("Squidex", "Squidex",
                            typeof(TelemetryServices).Assembly.GetName().Version!.ToString()));

                builder.AddSource("Squidex");

                builder.AddAspNetCoreInstrumentation();
                builder.AddHttpClientInstrumentation();
            });
        }
    }
}
