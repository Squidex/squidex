// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.APM.Otlp;

public sealed class OtlpPlugin : IPlugin
{
    private sealed class Configurator : ITelemetryConfigurator
    {
        private readonly IConfiguration config;

        public Configurator(IConfiguration config)
        {
            this.config = config;
        }

        public void Configure(TracerProviderBuilder builder)
        {
            builder.AddOtlpExporter(options =>
            {
                config.GetSection("logging:otlp").Bind(options);
            });
        }

        public void Configure(MeterProviderBuilder builder)
        {
            builder.AddOtlpExporter(options =>
            {
                config.GetSection("logging:otlp").Bind(options);
            });
        }
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        if (config.GetValue<bool>("logging:otlp:enabled"))
        {
            services.AddSingleton<ITelemetryConfigurator, Configurator>();
        }
    }
}
