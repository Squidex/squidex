// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Squidex.Infrastructure.Json;
using Squidex.Pipeline.Robots;
using Squidex.Web;
using Squidex.Web.Pipeline;

namespace Squidex.Config.Web
{
    public static class WebExtensions
    {
        public static IApplicationBuilder UseSquidexLocalCache(this IApplicationBuilder app)
        {
            app.UseMiddleware<LocalCacheMiddleware>();

            return app;
        }

        public static IApplicationBuilder UseSquidexLocalization(this IApplicationBuilder app)
        {
            var supportedCultures = new[] { "en", "nl", "it" };

            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            app.UseRequestLocalization(localizationOptions);

            return app;
        }

        public static IApplicationBuilder UseSquidexTracking(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestExceptionMiddleware>();
            app.UseMiddleware<RequestLogPerformanceMiddleware>();
            app.UseMiddleware<UsageMiddleware>();

            return app;
        }

        public static IApplicationBuilder UseSquidexHealthCheck(this IApplicationBuilder app)
        {
            var serializer = app.ApplicationServices.GetRequiredService<IJsonSerializer>();

            var writer = new Func<HttpContext, HealthReport, Task>((httpContext, report) =>
            {
                var response = new
                {
                    Entries = report.Entries.ToDictionary(x => x.Key, x =>
                    {
                        var value = x.Value;

                        return new
                        {
                            Data = value.Data.Count > 0 ? new Dictionary<string, object>(value.Data) : null,
                            value.Description,
                            value.Duration,
                            value.Status
                        };
                    }),
                    report.Status,
                    report.TotalDuration
                };

                var json = serializer.Serialize(response);

                httpContext.Response.Headers[HeaderNames.ContentType] = "text/json";

                return httpContext.Response.WriteAsync(json);
            });

            app.UseHealthChecks("/readiness", new HealthCheckOptions
            {
                Predicate = check => !check.Tags.Contains("background"),
                ResponseWriter = writer
            });

            app.UseHealthChecks("/healthz", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("node"),
                ResponseWriter = writer
            });

            app.UseHealthChecks("/cluster-healthz", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("cluster"),
                ResponseWriter = writer
            });

            app.UseHealthChecks("/background-healthz", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("background"),
                ResponseWriter = writer
            });

            return app;
        }

        public static IApplicationBuilder UseSquidexRobotsTxt(this IApplicationBuilder app)
        {
            app.Map("/robots.txt", builder => builder.UseMiddleware<RobotsTxtMiddleware>());

            return app;
        }

        public static void UseSquidexCors(this IApplicationBuilder app)
        {
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
        }

        public static void UseSquidexForwardingRules(this IApplicationBuilder app, IConfiguration config)
        {
            var urlsOptions = app.ApplicationServices.GetRequiredService<IOptions<UrlsOptions>>().Value;

            if (urlsOptions.EnableForwardHeaders)
            {
                var options = new ForwardedHeadersOptions
                {
                    AllowedHosts = new List<string>
                    {
                        new Uri(urlsOptions.BaseUrl).Host
                    },
                    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
                    ForwardLimit = null,
                    RequireHeaderSymmetry = false
                };

                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();

                if (urlsOptions.KnownProxies != null)
                {
                    foreach (var proxy in urlsOptions.KnownProxies)
                    {
                        if (IPAddress.TryParse(proxy, out var address))
                        {
                            options.KnownProxies.Add(address);
                        }
                    }
                }

                app.UseForwardedHeaders(options);
            }

            app.UseMiddleware<CleanupHostMiddleware>();

            if (urlsOptions.EnforceHost)
            {
                app.UseHostFiltering();
            }

            if (urlsOptions.EnforceHTTPS)
            {
                app.UseHttpsRedirection();
            }
        }
    }
}
