﻿// ==========================================================================
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
            Guard.NotNull(apiUsageTracker, nameof(apiUsageTracker));
            Guard.NotNull(appPlansProvider, nameof(appPlansProvider));
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.appPlansProvider = appPlansProvider;
            this.apiUsageTracker = apiUsageTracker;

            usageLimitNotifier = grainFactory.GetGrain<IUsageNotifierGrain>(SingleGrain.Id);
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

            if (plan.MaxApiCalls > 0 || plan.BlockingApiCalls > 0)
            {
                var usage = await apiUsageTracker.GetMonthCallsAsync(appId.ToString(), today, null);

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

                isBlocked = isBlocked || plan.BlockingApiCalls > 0 && usage > plan.BlockingApiCalls;
            }

            return isBlocked;
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
