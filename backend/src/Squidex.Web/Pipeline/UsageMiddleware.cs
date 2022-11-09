// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using NodaTime;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;

namespace Squidex.Web.Pipeline;

public sealed class UsageMiddleware : IMiddleware
{
    private readonly IAppLogStore usageLog;
    private readonly IUsageGate usageGate;

    public IClock Clock { get; set; } = SystemClock.Instance;

    public UsageMiddleware(IAppLogStore usageLog, IUsageGate usageGate)
    {
        this.usageLog = usageLog;
        this.usageGate = usageGate;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var usageBody = SetUsageBody(context);

        var watch = ValueStopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            if (context.Response.StatusCode != StatusCodes.Status429TooManyRequests)
            {
                var app = context.Features.Get<IAppFeature>()?.App;

                if (app != null)
                {
                    var bytes = usageBody.BytesWritten;

                    if (context.Request.ContentLength != null)
                    {
                        bytes += context.Request.ContentLength.Value;
                    }

                    var (_, clientId) = context.User.GetClient();

                    var request = default(RequestLog);

                    request.Bytes = bytes;
                    request.CacheStatus = "MISS";
                    request.CacheHits = 0;
                    request.Costs = context.Features.Get<IApiCostsFeature>()?.Costs ?? 0;
                    request.ElapsedMs = watch.Stop();
                    request.RequestMethod = context.Request.Method;
                    request.RequestPath = context.Request.Path;
                    request.Timestamp = Clock.GetCurrentInstant();
                    request.StatusCode = context.Response.StatusCode;
                    request.UserId = context.User.OpenIdSubject();
                    request.UserClientId = clientId;

                    // Do not flow cancellation token because it is too important.
                    await usageLog.LogAsync(app.Id, request, default);

                    if (request.Costs > 0)
                    {
                        var date = request.Timestamp.ToDateTimeUtc().Date;

                        await usageGate.TrackRequestAsync(app, request.UserClientId, date,
                            request.Costs,
                            request.ElapsedMs,
                            request.Bytes,
                            default);
                    }
                }
            }
        }
    }

    private static UsageResponseBodyFeature SetUsageBody(HttpContext context)
    {
        var originalBodyFeature = context.Features.Get<IHttpResponseBodyFeature>()!;

        var usageBody = new UsageResponseBodyFeature(originalBodyFeature);

        context.Features.Set<IHttpResponseBodyFeature>(usageBody);

        return usageBody;
    }
}
