// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.APM.Datadog
{
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
                // See: https://docs.microsoft.com/aspnet/core/grpc/troubleshoot#call-insecure-grpc-services-with-net-core-client
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

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
                services.AddSingleton<ITelemetryConfigurator,
                    Configurator>();
            }
        }
    }
}
