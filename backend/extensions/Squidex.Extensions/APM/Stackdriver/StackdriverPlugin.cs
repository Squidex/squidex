// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using Google.Cloud.Diagnostics.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Plugins;
using Squidex.Log;

namespace Squidex.Extensions.APM.Stackdriver
{
    public sealed class StackdriverPlugin : IPlugin
    {
        private sealed class Configurator : ITelemetryConfigurator
        {
            private readonly string projectId;

            public Configurator(string projectId)
            {
                this.projectId = projectId;
            }

            public void Configure(TracerProviderBuilder builder)
            {
                builder.UseStackdriverExporter(projectId);
            }
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var isEnabled = config.GetValue<bool>("logging:stackdriver:enabled");

            if (isEnabled)
            {
                var projectId = config.GetValue<string>("logging:stackdriver:projectId");

                if (!string.IsNullOrWhiteSpace(projectId))
                {
                    services.AddSingleton<ITelemetryConfigurator>(
                        new Configurator(projectId));

                    services.AddSingleton<ILogAppender,
                        StackdriverSeverityLogAppender>();

                    services.AddSingleton<ILogAppender,
                        StackdriverExceptionHandler>();

                    var serviceName = config.GetValue<string>("logging:name") ?? "Squidex";
                    var serviceVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();

                    services.AddSingleton(c => ContextExceptionLogger.Create(projectId, serviceVersion, serviceVersion, null));
                }
            }
        }
    }
}
