﻿// ==========================================================================
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
        private readonly IAppLogStore logStore;
        private readonly IApiUsageTracker usageTracker;
        private readonly IClock clock;

        public UsageMiddleware(IAppLogStore logStore, IApiUsageTracker usageTracker, IClock clock)
        {
            Guard.NotNull(logStore, nameof(logStore));
            Guard.NotNull(usageTracker, nameof(usageTracker));
            Guard.NotNull(clock, nameof(clock));

            this.logStore = logStore;

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

                    if (appId != null)
                    {
                        var bytes = usageBody.BytesWritten;

                        if (context.Request.ContentLength != null)
                        {
                            bytes += context.Request.ContentLength.Value;
                        }

                        var (_, clientId) = context.User.GetClient();

                        var request = default(RequestLog);

                        request.Bytes = bytes;
                        request.Costs = context.Features.Get<IApiCostsFeature>()?.Costs ?? 0;
                        request.ElapsedMs = watch.Stop();
                        request.RequestMethod = context.Request.Method;
                        request.RequestPath = context.Request.Path;
                        request.Timestamp = clock.GetCurrentInstant();
                        request.UserClientId = clientId;
                        request.UserId = context.User.OpenIdSubject();

                        await logStore.LogAsync(appId.Id, request);

                        if (request.Costs > 0)
                        {
                            var date = request.Timestamp.ToDateTimeUtc().Date;

                            await usageTracker.TrackAsync(date, appId.Id.ToString(),
                                request.UserClientId,
                                request.Costs,
                                request.ElapsedMs,
                                request.Bytes);
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