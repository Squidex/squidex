// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.APM.ApplicationInsights
{
    public sealed class ApplicationInsightsPlugin : IPlugin
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
                builder.AddAzureMonitorTraceExporter(options =>
                {
                    config.GetSection("logging:applicationInsights").Bind(options);
                });
            }
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            if (config.GetValue<bool>("logging:applicationInsights:enabled"))
            {
                services.AddSingleton<ITelemetryConfigurator,
                    Configurator>();
            }
        }
    }
}
