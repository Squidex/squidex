// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Diagnostics;

namespace Squidex.Pipeline.Diagnostics
{
    public sealed class HealthCheckMiddleware : IMiddleware
    {
        private const string Suffix = "HealthCheck";
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

        private readonly Dictionary<string, IHealthCheck> healthChecks;
        private readonly JsonSerializerSettings serializerSettings;

        public HealthCheckMiddleware(IEnumerable<IHealthCheck> healthChecks, JsonSerializerSettings serializerSettings)
        {
            Guard.NotNull(healthChecks, nameof(healthChecks));
            Guard.NotNull(serializerSettings, nameof(serializerSettings));

            this.healthChecks = healthChecks.ToDictionary(GetName);
            this.serializerSettings = serializerSettings;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (CanServeRequest(context.Request))
            {
                using (var cts = new CancellationTokenSource(Timeout))
                {
                    var checks = await Task.WhenAll(healthChecks.Select(x => MakeHealthCheckAsync(x.Key, x.Value, cts.Token)));

                    context.Response.StatusCode = 200;
                    context.Response.Headers.Add("Content-Type", "application/json");

                    if (checks.Any(x => !x.Result.IsHealthy))
                    {
                        context.Response.StatusCode = 503;
                    }

                    var response = checks.ToDictionary(x => x.Name, x => x.Result);

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { status = response }, Formatting.Indented, serializerSettings));
                }
            }
            else
            {
                await next(context);
            }
        }

        private static bool CanServeRequest(HttpRequest request)
        {
            return HttpMethods.IsGet(request.Method) && (request.Path == "/" || string.IsNullOrEmpty(request.Path));
        }

        private static string GetName(IHealthCheck check)
        {
            var name = check.GetType().Name.ToCamelCase();

            if (name.EndsWith(Suffix, StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - Suffix.Length);
            }

            return name;
        }

        private async Task<(string Name, HealthCheckResult Result)> MakeHealthCheckAsync(string name, IHealthCheck check, CancellationToken ct)
        {
            try
            {
                var result = await check.CheckHealthAsync(ct);

                return (name, result);
            }
            catch
            {
                return (name, new HealthCheckResult(false));
            }
        }
    }
}
