// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Orleans;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public class UsageGate
    {
        private readonly MemoryCache memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly IAppPlansProvider appPlansProvider;
        private readonly IApiUsageTracker apiUsageTracker;
        private readonly IGrainFactory grainFactory;

        public UsageGate(IAppPlansProvider appPlansProvider, IApiUsageTracker apiUsageTracker, IGrainFactory grainFactory)
        {
            this.appPlansProvider = appPlansProvider;
            this.apiUsageTracker = apiUsageTracker;
            this.grainFactory = grainFactory;
        }

        public virtual async Task<bool> IsBlockedAsync(IAppEntity app, string? clientId, DateTime today)
        {
            Guard.NotNull(app, nameof(app));

            var appId = app.Id;

            var isBlocked = false;

            if (clientId != null && app.Clients.TryGetValue(clientId, out var client) && client.ApiCallsLimit > 0)
            {
                var usage = await apiUsageTracker.GetMonthCallsAsync(appId.ToString(), today, clientId);

                isBlocked = usage >= client.ApiCallsLimit;
            }

            var (plan, _) = appPlansProvider.GetPlanForApp(app);

            var limit = plan.MaxApiCalls;

            if (limit > 0 || plan.BlockingApiCalls > 0)
            {
                var usage = await apiUsageTracker.GetMonthCallsAsync(appId.ToString(), today, null);

                if (IsOver10Percent(limit, usage) && IsAboutToBeLocked(today, limit, usage) && !HasNotifiedBefore(app.Id))
                {
                    var notification = new UsageNotification
                    {
                        AppId = appId,
                        AppName = app.Name,
                        Usage = usage,
                        UsageLimit = limit,
                        Users = GetUsers(app)
                    };

                    GetGrain().NotifyAsync(notification).Forget();

                    TrackNotified(appId);
                }

                isBlocked = isBlocked || (plan.BlockingApiCalls > 0 && usage > plan.BlockingApiCalls);
            }

            return isBlocked;
        }

        private IUsageNotifierGrain GetGrain()
        {
            return grainFactory.GetGrain<IUsageNotifierGrain>(SingleGrain.Id);
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
