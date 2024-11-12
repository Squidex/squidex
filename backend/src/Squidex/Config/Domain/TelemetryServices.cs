// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Squidex.Infrastructure;

namespace Squidex.Config.Domain;

public static class TelemetryServices
{
    public static void AddSquidexTelemetry(this IServiceCollection services, IConfiguration config)
    {
        var serviceName = config.GetValue<string>("logging:name") ?? "Squidex";
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName, "Squidex",
                typeof(TelemetryServices).Assembly.GetName().Version!.ToString());

        services.AddOpenTelemetry();

        // Configure logging
        services.AddLogging(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);
                options.IncludeFormattedMessage = true;

                // Add OTLP exporter and bind options directly. Sadly not possible
                // to do it through ITelemetryConfigurator as it is not possible to
                // get IServiceProvider here. Later when OpenTelemetry.Sdk.CreateLoggerProviderBuilder()
                // is available and no longer expermential, we can do it the same way as with tracing and metrics...
                if (config.GetValue<bool>("logging:otlp:enabled"))
                {
                    options.AddOtlpExporter(options =>
                    {
                        config.GetSection("logging:otlp").Bind(options);
                    });
                }
            });
        });

        // Configure tracing
        services.AddSingleton(serviceProvider =>
        {
            var builder = Sdk.CreateTracerProviderBuilder();

            builder.SetResourceBuilder(resourceBuilder);
            builder.AddSource("Squidex");
            builder.AddAspNetCoreInstrumentation();
            builder.AddHttpClientInstrumentation();

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

            return builder.Build()!;
        });

        // Configure metrics
        services.AddSingleton(serviceProvider =>
        {
            var builder = Sdk.CreateMeterProviderBuilder();

            builder.SetResourceBuilder(resourceBuilder);
            builder.AddAspNetCoreInstrumentation();
            builder.AddHttpClientInstrumentation();
            builder.AddRuntimeInstrumentation();

            foreach (var configurator in serviceProvider.GetRequiredService<IEnumerable<ITelemetryConfigurator>>())
            {
                configurator.Configure(builder);
            }

            return builder.Build()!;
        });
    }
}
