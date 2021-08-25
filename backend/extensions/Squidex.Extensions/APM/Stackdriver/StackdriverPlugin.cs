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
using Squidex.Infrastructure.Plugins;
using Squidex.Log;

namespace Squidex.Extensions.APM.Stackdriver
{
    public sealed class StackdriverPlugin : IPlugin
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var projectId = config.GetValue<string>("logging:stackdriver:projectId");
            var projectName = config.GetValue<string>("logging:stackdriver:projectName") ?? "Squidex";

            var isEnabled = config.GetValue<bool>("logging:stackdriver:enabled");

            if (isEnabled && !string.IsNullOrWhiteSpace(projectId))
            {
                services.AddOpenTelemetryTracing(builder =>
                {
                    builder.UseStackdriverExporter(projectId);
                });

                services.AddSingleton<ILogAppender,
                    StackdriverSeverityLogAppender>();

                services.AddSingleton<ILogAppender,
                    StackdriverExceptionHandler>();

                var version = Assembly.GetEntryAssembly().GetName().Version?.ToString();

                services.AddSingleton(c => ContextExceptionLogger.Create(projectId, projectName, version, null));
            }
        }
    }
}
