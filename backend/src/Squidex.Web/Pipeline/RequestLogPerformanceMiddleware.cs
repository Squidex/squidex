// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Log;

namespace Squidex.Web.Pipeline;

public sealed class RequestLogPerformanceMiddleware
{
    private readonly RequestDelegate next;

    public RequestLogPerformanceMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISemanticLog log, IOptions<RequestLogOptions> requestLogOptions)
    {
        if (!requestLogOptions.Value.LogRequests)
        {
            await next(context);
            return;
        }

        var watch = ValueStopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            var elapsedMs = watch.Stop();

            log.LogInformation((elapsedMs, context), (ctx, w) =>
            {
                w.WriteProperty("message", "HTTP request executed.");
                w.WriteProperty("elapsedRequestMs", ctx.elapsedMs);
                w.WriteObject("filters", ctx.context, LogFilters);
            });
        }
    }

    private static void LogFilters(HttpContext httpContext, IObjectWriter obj)
    {
        var app = httpContext.Context().App;

        if (app != null)
        {
            obj.WriteProperty("appId", app.Id.ToString());
            obj.WriteProperty("appName", app.Name);
        }

        var userId = httpContext.User.OpenIdSubject();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            obj.WriteProperty(nameof(userId), userId);
        }

        var clientId = httpContext.User.OpenIdClientId();

        if (!string.IsNullOrWhiteSpace(clientId))
        {
            obj.WriteProperty(nameof(clientId), clientId);
        }

        var costs = httpContext.Features.Get<IApiCostsFeature>()?.Costs ?? 0;

        obj.WriteProperty(nameof(costs), costs);
    }
}
