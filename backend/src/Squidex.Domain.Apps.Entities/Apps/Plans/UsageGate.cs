// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public class UsageGate
    {
        private readonly MemoryCache memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly IAppPlansProvider appPlansProvider;
        private readonly IApiUsageTracker apiUsageTracker;
        private readonly IBus bus;

        public UsageGate(IAppPlansProvider appPlansProvider, IApiUsageTracker apiUsageTracker, IBus bus)
        {
            this.appPlansProvider = appPlansProvider;
            this.apiUsageTracker = apiUsageTracker;
            this.bus = bus;
        }

        public virtual async Task<bool> IsBlockedAsync(IAppEntity app, string? clientId, DateTime today,
            CancellationToken ct = default)
        {
            Guard.NotNull(app);

            var appId = app.Id;

            var isBlocked = false;

            if (clientId != null && app.Clients.TryGetValue(clientId, out var client) && client.ApiCallsLimit > 0)
            {
                var usage = await apiUsageTracker.GetMonthCallsAsync(appId.ToString(), today, clientId, ct);

                isBlocked = usage >= client.ApiCallsLimit;
            }

            var (plan, _) = appPlansProvider.GetPlanForApp(app);

            var limit = plan.MaxApiCalls;

            if (limit > 0 || plan.BlockingApiCalls > 0)
            {
                var usage = await apiUsageTracker.GetMonthCallsAsync(appId.ToString(), today, null, ct);

                if (IsOver10Percent(limit, usage) && IsAboutToBeLocked(today, limit, usage) && !HasNotifiedBefore(app.Id))
                {
                    var notification = new UsageTrackingCheck
                    {
                        AppId = appId,
                        AppName = app.Name,
                        Usage = usage,
                        UsageLimit = limit,
                        Users = GetUsers(app)
                    };

                    await bus.Publish(notification, ct);

                    TrackNotified(appId);
                }

                isBlocked = isBlocked || (plan.BlockingApiCalls > 0 && usage > plan.BlockingApiCalls);
            }

            return isBlocked;
        }

        private bool HasNotifiedBefore(DomainId appId)
        {
            return memoryCache.Get<bool>(appId);
        }

        private bool TrackNotified(DomainId appId)
        {
            return memoryCache.Set(appId, true, TimeSpan.FromHours(1));
        }

        private static string[] GetUsers(IAppEntity app)
        {
            return app.Contributors.Where(x => x.Value == Role.Owner).Select(x => x.Key).ToArray();
        }

        private static bool IsOver10Percent(long limit, long usage)
        {
            return usage > limit * 0.1;
        }

        private static bool IsAboutToBeLocked(DateTime today, long limit, long usage)
        {
            var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

            var forecasted = ((float)usage / today.Day) * daysInMonth;

            return forecasted > limit;
        }
    }
}
