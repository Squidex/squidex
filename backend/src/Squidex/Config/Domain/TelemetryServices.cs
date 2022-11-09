// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Squidex.Infrastructure;

namespace Squidex.Config.Domain;

public static class TelemetryServices
{
    public static void AddSquidexTelemetry(this IServiceCollection services, IConfiguration config)
    {
        services.AddOpenTelemetryTracing();

        services.AddSingleton(serviceProvider =>
        {
            var builder = Sdk.CreateTracerProviderBuilder();

            var serviceName = config.GetValue<string>("logging:name") ?? "Squidex";

            builder.SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(serviceName, "Squidex",
                        typeof(TelemetryServices).Assembly.GetName().Version!.ToString()));

            builder.AddSource("Squidex");

            builder.AddAspNetCoreInstrumentation();
            builder.AddHttpClientInstrumentation();
            builder.AddMongoDBInstrumentation();

            var sampling = config.GetValue<double>("logging:otlp:sampling");

            if (sampling > 0 && sampling < 1)
            {
                builder.SetSampler(
                    new ParentBasedSampler(
                        new TraceIdRatioBasedSampler(sampling)));
            }

            foreach (var configurator in serviceProvider.GetRequiredService<IEnumerable<ITelemetryConfigurator>>())
            {
                configurator.Configure(builder);
            }

            return builder.Build();
        });
    }
}
