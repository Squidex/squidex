// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
            services.AddOpenTelemetryTracing(builder =>
            {
                if (config.GetValue<bool>("logging:stackdriver:enabled"))
                {
                    var projectId = config.GetValue<string>("logging:stackdriver:projectId");

                    builder.UseStackdriverExporter(projectId);

                    services.AddSingleton<ILogAppender,
                        StackdriverSeverityLogAppender>();

                    services.AddSingleton<ILogAppender,
                        StackdriverExceptionHandler>();
                }
            });
        }
    }
}
