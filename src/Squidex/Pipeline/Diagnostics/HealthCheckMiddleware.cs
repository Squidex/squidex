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
    public sealed class HealthCheckMiddleware
    {
        private const string Suffix = "HealthCheck";
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

        private readonly Dictionary<string, IHealthCheck> healthChecks;
        private readonly JsonSerializerSettings serializerSettings;
        private readonly RequestDelegate next;
        private readonly List<string> scopes;

        public HealthCheckMiddleware(IEnumerable<IHealthCheck> healthChecks, JsonSerializerSettings serializerSettings, RequestDelegate next, string scopes)
        {
            Guard.NotNull(healthChecks, nameof(healthChecks));
            Guard.NotNull(serializerSettings, nameof(serializerSettings));

            this.healthChecks = healthChecks.ToDictionary(GetName);
            this.next = next;
            this.scopes = SplitScopes(scopes);
            this.serializerSettings = serializerSettings;
        }

        public async Task Invoke(HttpContext context)
        {
            if (CanServeRequest(context.Request))
            {
                using (var cts = new CancellationTokenSource(Timeout))
                {
                    var matchingChecks = healthChecks.Where(x => CanUseCheck(x.Value));

                    var results = await Task.WhenAll(matchingChecks.Select(x => MakeHealthCheckAsync(x.Key, x.Value, cts.Token)));

                    context.Response.StatusCode = 200;
                    context.Response.Headers.Add("Content-Type", "application/json");

                    if (results.Any(x => !x.Result.IsHealthy))
                    {
                        context.Response.StatusCode = 503;
                    }

                    var response = results.ToDictionary(x => x.Name, x => x.Result);

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { status = response }, Formatting.Indented, serializerSettings));
                }
            }
            else
            {
                await next(context);
            }
        }

        private bool CanUseCheck(IHealthCheck check)
        {
            return scopes.Count == 0 || check.Scopes.Intersect(scopes).Any();
        }

        private bool CanServeRequest(HttpRequest request)
        {
            return HttpMethods.IsGet(request.Method) && (request.Path == "/" || string.IsNullOrEmpty(request.Path));
        }

        private static List<string> SplitScopes(string scopes)
        {
            return scopes.Split(",").Where(x => x != "*").ToList();
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
