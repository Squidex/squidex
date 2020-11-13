// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Plugins;
using Squidex.Log;

namespace Squidex.Extensions.APM.ApplicationInsights
{
    public sealed class ApplicationInsightsPlugin : IPlugin, IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                var client = builder.ApplicationServices.GetRequiredService<TelemetryClient>();

                Profiler.SpanStarted += session =>
                {
                    session.Listen(client.StartOperation<RequestTelemetry>(session.Key));
                };

                next(builder);
            };
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var isEnabled = config.GetValue<bool>("logging:applicationInsights");

            if (isEnabled)
            {
                services.AddSingleton<IStartupFilter>(this);
                services.AddApplicationInsightsTelemetry();
                services.AddSingleton<ITelemetryInitializer, RoleNameTelemetryInitializer>();
            }
        }
    }
}
