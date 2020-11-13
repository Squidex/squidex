// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Datadog.Trace;
using Datadog.Trace.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Plugins;
using Squidex.Log;

namespace Squidex.Extensions.APM.Datadog
{
    public sealed class DatadogPlugin : IPlugin, IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.Use(async (context, next) =>
                {
                    using (var scope = Tracer.Instance.StartActive(context.Request.Path))
                    {
                        try
                        {
                            scope.Span.SetTag("http.method", context.Request.Method);

                            await next();
                        }
                        catch (Exception ex)
                        {
                            scope.Span.SetException(ex);
                            throw;
                        }
                        finally
                        {
                            scope.Span.SetTag("http.status_code", context.Response.StatusCode.ToString());
                        }
                    }
                });

                next(builder);
            };
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var isEnabled = config.GetValue<bool>("logging:datadog");

            if (isEnabled)
            {
                services.AddSingleton<IStartupFilter>(this);

                SetupTracer();
                SetupProfiler();
            }
        }

        private static void SetupProfiler()
        {
            Profiler.SpanStarted += session =>
            {
                session.Listen(Tracer.Instance.StartActive(session.Key));
            };
        }

        private static void SetupTracer()
        {
            var settings = TracerSettings.FromDefaultSources();

            settings.ServiceName = "squidex";

            Tracer.Instance = new Tracer(settings);
        }
    }
}
