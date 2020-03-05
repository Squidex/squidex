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
        private readonly IUsageNotifierGrain usageLimitNotifier;

        public UsageGate(IAppPlansProvider appPlansProvider, IApiUsageTracker apiUsageTracker, IGrainFactory grainFactory)
        {
            Guard.NotNull(apiUsageTracker);
            Guard.NotNull(appPlansProvider);
            Guard.NotNull(grainFactory);

            this.appPlansProvider = appPlansProvider;
            this.apiUsageTracker = apiUsageTracker;

            usageLimitNotifier = grainFactory.GetGrain<IUsageNotifierGrain>(SingleGrain.Id);
        }

        public virtual async Task<bool> IsBlockedAsync(IAppEntity app, DateTime today)
        {
            Guard.NotNull(app);

            var isLocked = false;

            var (plan, _) = appPlansProvider.GetPlanForApp(app);

            if (plan.MaxApiCalls > 0 || plan.BlockingApiCalls > 0)
            {
                var appId = app.Id;

                var usage = await apiUsageTracker.GetMonthCostsAsync(appId.ToString(), today);

                if (IsAboutToBeLocked(today, plan.MaxApiCalls, usage) && !HasNotifiedBefore(app.Id))
                {
                    var users = app.Contributors.Where(x => x.Value == Role.Owner).Select(x => x.Key).ToArray();

                    var notification = new UsageNotification
                    {
                        AppId = appId,
                        AppName = app.Name,
                        Usage = usage,
                        UsageLimit = plan.MaxApiCalls,
                        Users = users
                    };

                    usageLimitNotifier.NotifyAsync(notification).Forget();

                    TrackNotified(appId);
                }

                isLocked = plan.BlockingApiCalls > 0 && usage > plan.BlockingApiCalls;
            }

            return isLocked;
        }

        private bool HasNotifiedBefore(Guid appId)
        {
            return memoryCache.Get<bool>(appId);
        }

        private bool TrackNotified(Guid appId)
        {
            return memoryCache.Set(appId, true, TimeSpan.FromHours(1));
        }

        private bool IsAboutToBeLocked(DateTime today, long limit, long usage)
        {
            var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

            var forecasted = ((float)usage / today.Day) * daysInMonth;

            return forecasted > limit;
        }
    }
}
