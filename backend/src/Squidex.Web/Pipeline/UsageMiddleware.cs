// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using NodaTime;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Web.Pipeline
{
    public sealed class UsageMiddleware : IMiddleware
    {
        private readonly IAppLogStore log;
        private readonly IApiUsageTracker usageTracker;
        private readonly IClock clock;

        public UsageMiddleware(IAppLogStore log, IApiUsageTracker usageTracker, IClock clock)
        {
            Guard.NotNull(log);
            Guard.NotNull(usageTracker);
            Guard.NotNull(clock);

            this.log = log;

            this.usageTracker = usageTracker;

            this.clock = clock;
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
                    var appId = context.Features.Get<IAppFeature>()?.AppId;

                    var costs = context.Features.Get<IApiCostsFeature>()?.Costs ?? 0;

                    if (appId != null)
                    {
                        var elapsedMs = watch.Stop();

                        var now = clock.GetCurrentInstant();

                        var userId = context.User.OpenIdSubject();
                        var userClient = context.User.OpenIdClientId();

                        await log.LogAsync(appId.Id, now,
                            context.Request.Method,
                            context.Request.Path,
                            userId,
                            userClient,
                            elapsedMs,
                            costs);

                        if (costs > 0)
                        {
                            var bytes = usageBody.BytesWritten;

                            if (context.Request.ContentLength != null)
                            {
                                bytes += context.Request.ContentLength.Value;
                            }

                            var date = now.ToDateTimeUtc().Date;

                            await usageTracker.TrackAsync(date, appId.Id.ToString(), userClient, costs, elapsedMs, bytes);
                        }
                    }
                }
            }
        }

        private static UsageResponseBodyFeature SetUsageBody(HttpContext context)
        {
            var originalBodyFeature = context.Features.Get<IHttpResponseBodyFeature>();

            var usageBody = new UsageResponseBodyFeature(originalBodyFeature);

            context.Features.Set<IHttpResponseBodyFeature>(usageBody);

            return usageBody;
        }
    }
}