// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Billing;

public sealed class UsageGate : IUsageGate, IAssetUsageTracker
{
    private const string CounterTotalCount = "TotalAssets";
    private const string CounterTotalSize = "TotalSize";
    private static readonly DateTime SummaryDate = default;
    private readonly IApiUsageTracker apiUsageTracker;
    private readonly IAppProvider appProvider;
    private readonly IBillingPlans billingPlans;
    private readonly IMemoryCache memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
    private readonly IMessageBus messaging;
    private readonly IUsageTracker usageTracker;

    public UsageGate(
        IAppProvider appProvider,
        IApiUsageTracker apiUsageTracker,
        IBillingPlans billingPlans,
        IMessageBus messaging,
        IUsageTracker usageTracker)
    {
        this.appProvider = appProvider;
        this.apiUsageTracker = apiUsageTracker;
        this.billingPlans = billingPlans;
        this.messaging = messaging;
        this.usageTracker = usageTracker;
    }

    public Task DeleteAssetUsageAsync(DomainId appId,
        CancellationToken ct = default)
    {
        // Do not delete the team, as this is only called when an app is deleted.
        return usageTracker.DeleteAsync(AppAssetsKey(appId), ct);
    }

    public Task DeleteAssetsUsageAsync(
        CancellationToken ct = default)
    {
        // Use a well defined prefix query for the deletion to improve performance.
        return usageTracker.DeleteByKeyPatternAsync("^([a-zA-Z0-9]+)_Assets", ct);
    }

    public Task<long> GetTotalSizeByAppAsync(DomainId appId,
        CancellationToken ct = default)
    {
        return GetTotalSizeAsync(AppAssetsKey(appId), ct);
    }

    public Task<long> GetTotalSizeByTeamAsync(DomainId teamId,
        CancellationToken ct = default)
    {
        return GetTotalSizeAsync(TeamAssetsKey(teamId), ct);
    }

    private async Task<long> GetTotalSizeAsync(string key,
        CancellationToken ct)
    {
        var counters = await usageTracker.GetAsync(key, SummaryDate, SummaryDate, null, ct);

        return counters.GetInt64(CounterTotalSize);
    }

    public Task<IReadOnlyList<AssetStats>> QueryByAppAsync(DomainId appId, DateTime fromDate, DateTime toDate,
        CancellationToken ct = default)
    {
        return QueryAsync(AppAssetsKey(appId), fromDate, toDate, ct);
    }

    public Task<IReadOnlyList<AssetStats>> QueryByTeamAsync(DomainId teamId, DateTime fromDate, DateTime toDate,
        CancellationToken ct = default)
    {
        return QueryAsync(TeamAssetsKey(teamId), fromDate, toDate, ct);
    }

    private async Task<IReadOnlyList<AssetStats>> QueryAsync(string key, DateTime fromDate, DateTime toDate,
        CancellationToken ct)
    {
        var enriched = new List<AssetStats>();

        var usages = await usageTracker.QueryAsync(key, fromDate, toDate, ct);

        if (usages.TryGetValue(usageTracker.FallbackCategory, out var byCategory1))
        {
            AddCounters(enriched, byCategory1);
        }

        return enriched;
    }

    private static void AddCounters(List<AssetStats> enriched, List<(DateTime, Counters)> details)
    {
        foreach (var (date, counters) in details)
        {
            var totalCount = counters.GetInt64(CounterTotalCount);
            var totalSize = counters.GetInt64(CounterTotalSize);

            enriched.Add(new AssetStats(date, totalCount, totalSize));
        }
    }

    public async Task TrackRequestAsync(IAppEntity app, string? clientId, DateTime date, double costs, long elapsedMs, long bytes,
       CancellationToken ct = default)
    {
        var appId = app.Id.ToString();

        if (app.TeamId != null)
        {
            await apiUsageTracker.TrackAsync(date, app.TeamId.ToString()!, app.Name, costs, elapsedMs, bytes, ct);
        }

        await apiUsageTracker.TrackAsync(date, appId, clientId, costs, elapsedMs, bytes, ct);
    }

    public async Task<bool> IsBlockedAsync(IAppEntity app, string? clientId, DateTime date,
        CancellationToken ct = default)
    {
        Guard.NotNull(app);

        // Resolve the plan from either the app or the assigned team.
        var (plan, _, teamId) = await GetPlanForAppAsync(app, true, ct);

        var appId = app.Id;
        var blocking = false;
        var blockLimit = plan.MaxApiCalls;
        var referenceId = teamId ?? app.Id;

        if (blockLimit > 0 || plan.BlockingApiCalls > 0)
        {
            var usage = await apiUsageTracker.GetMonthCallsAsync(referenceId.ToString(), date, null, ct);

            if (IsOver10Percent(blockLimit, usage) && IsAboutToBeLocked(date, blockLimit, usage) && !HasNotifiedBefore(appId))
            {
                var notification = new UsageTrackingCheck
                {
                    AppId = appId,
                    Usage = usage,
                    UsageLimit = blockLimit,
                    Users = GetUsers(app)
                };

                await messaging.PublishAsync(notification, ct: ct);

                TrackNotified(appId);
            }

            blocking = plan.BlockingApiCalls > 0 && usage > plan.BlockingApiCalls;
        }

        if (!blocking)
        {
            if (clientId != null && app.Clients.TryGetValue(clientId, out var client) && client.ApiCallsLimit > 0)
            {
                var usage = await apiUsageTracker.GetMonthCallsAsync(appId.ToString(), date, clientId, ct);

                blocking = usage >= client.ApiCallsLimit;
            }
        }

        return blocking;
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

    public async Task TrackAssetAsync(DomainId appId, DateTime date, long fileSize, long count,
        CancellationToken ct = default)
    {
        var counters = new Counters
        {
            [CounterTotalSize] = fileSize,
            [CounterTotalCount] = count
        };

        var appKey = AppAssetsKey(appId);

        var tasks = new List<Task>
        {
            usageTracker.TrackAsync(date, appKey, null, counters, ct),
            usageTracker.TrackAsync(SummaryDate, appKey, null, counters, ct)
        };

        var (_, _, teamId) = await GetPlanForAppAsync(appId, true, ct);

        if (teamId != null)
        {
            var teamKey = TeamAssetsKey(teamId.Value);

            tasks.Add(usageTracker.TrackAsync(date, teamKey, null, counters, ct));
            tasks.Add(usageTracker.TrackAsync(SummaryDate, teamKey, null, counters, ct));
        }

        await Task.WhenAll(tasks);
    }

    public Task<(Plan Plan, string PlanId, DomainId? TeamId)> GetPlanForAppAsync(IAppEntity app, bool canCache,
        CancellationToken ct = default)
    {
        Guard.NotNull(app);

        if (!canCache)
        {
            return GetPlanCoreAsync(app, ct);
        }

        return memoryCache.GetOrCreateAsync(CacheKey(app.Id), async x =>
        {
            x.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            return await GetPlanCoreAsync(app, ct);
        });
    }

    public Task<(Plan Plan, string PlanId, DomainId? TeamId)> GetPlanForAppAsync(DomainId appId, bool canCache,
        CancellationToken ct = default)
    {
        if (!canCache)
        {
            return GetPlanCoreAsync(appId, ct);
        }

        return memoryCache.GetOrCreateAsync(CacheKey(appId), async x =>
        {
            x.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            return await GetPlanCoreAsync(appId, ct);
        });
    }

    private async Task<(Plan Plan, string PlanId, DomainId? TeamId)> GetPlanCoreAsync(DomainId appId,
        CancellationToken ct)
    {
        var app = await appProvider.GetAppAsync(appId, true, ct);

        if (app == null)
        {
            var freePlan = billingPlans.GetFreePlan();

            return (freePlan, freePlan.Id, null);
        }

        return await GetPlanCoreAsync(app, ct);
    }

    private async Task<(Plan Plan, string PlanId, DomainId? TeamId)> GetPlanCoreAsync(IAppEntity app,
        CancellationToken ct)
    {
        if (app.TeamId != null)
        {
            var team = await appProvider.GetTeamAsync(app.TeamId.Value, ct);

            var (plan, planId) = billingPlans.GetActualPlan(team?.Plan?.PlanId ?? app.Plan?.PlanId);

            return (plan, planId, team?.Id);
        }
        else
        {
            var (plan, planId) = billingPlans.GetActualPlan(app.Plan?.PlanId);

            return (plan, planId, null);
        }
    }

    public Task<(Plan Plan, string PlanId)> GetPlanForTeamAsync(ITeamEntity team,
        CancellationToken ct = default)
    {
        var (plan, planId) = billingPlans.GetActualPlan(team?.Plan?.PlanId);

        return Task.FromResult((plan, planId));
    }

    private static string AppAssetsKey(DomainId appId)
    {
        return $"{appId}_Assets";
    }

    private static string TeamAssetsKey(DomainId appId)
    {
        return $"{appId}_TeamAssets";
    }

    private static string CacheKey(DomainId appId)
    {
        return $"{appId}_Plan";
    }
}
