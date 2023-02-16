// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Domain.Apps.Entities.Billing;

public sealed partial class UsageGate : IAssetUsageTracker
{
    public static class AssetsKeys
    {
        public const string TotalAssets = nameof(AssetCounters.TotalAssets);
        public const string TotalSize = nameof(AssetCounters.TotalSize);
    }

    Task IAssetUsageTracker.DeleteUsageAsync(DomainId appId,
        CancellationToken ct)
    {
        // Do not delete the team, as this is only called when an app is deleted.
        return usageTracker.DeleteAsync(AppAssetsKey(appId), ct);
    }

    Task IAssetUsageTracker.DeleteUsageAsync(
        CancellationToken ct)
    {
        // Use a well defined prefix query for the deletion to improve performance.
        return usageTracker.DeleteByKeyPatternAsync("^([a-zA-Z0-9]+)_[A-Za-z]+Assets", ct);
    }

    Task<AssetCounters> IAssetUsageTracker.GetTotalByAppAsync(DomainId appId,
        CancellationToken ct)
    {
        return GetTotalForAssetsAsync(AppAssetsKey(appId), ct);
    }

    Task<AssetCounters> IAssetUsageTracker.GetTotalByTeamAsync(DomainId teamId,
        CancellationToken ct)
    {
        return GetTotalForAssetsAsync(TeamAssetsKey(teamId), ct);
    }

    Task<IReadOnlyList<AssetStats>> IAssetUsageTracker.QueryByAppAsync(DomainId appId, DateOnly fromDate, DateOnly toDate,
        CancellationToken ct)
    {
        return QueryForAssetsAsync(AppAssetsKey(appId), fromDate, toDate, ct);
    }

    Task<IReadOnlyList<AssetStats>> IAssetUsageTracker.QueryByTeamAsync(DomainId teamId, DateOnly fromDate, DateOnly toDate,
        CancellationToken ct)
    {
        return QueryForAssetsAsync(TeamAssetsKey(teamId), fromDate, toDate, ct);
    }

    private async Task<AssetCounters> GetTotalForAssetsAsync(string key,
        CancellationToken ct)
    {
        var counters = await usageTracker.GetAsync(key, SummaryDate, SummaryDate, null, ct);

        return GetAssetCounters(counters);
    }

    private async Task<IReadOnlyList<AssetStats>> QueryForAssetsAsync(string key, DateOnly fromDate, DateOnly toDate,
        CancellationToken ct)
    {
        var result = new List<AssetStats>();

        var usages = await usageTracker.QueryAsync(key, fromDate, toDate, ct);

        for (var date = fromDate; date <= toDate; date = date.AddDays(1))
        {
            var aggregated = default(AssetCounters);

            foreach (var (_, byCategory) in usages)
            {
                foreach (var (counterDate, counters) in byCategory)
                {
                    if (counterDate == date)
                    {
                        var currentCounters = GetAssetCounters(counters);

                        aggregated.TotalSize += currentCounters.TotalSize;
                        aggregated.TotalAssets += currentCounters.TotalAssets;
                    }
                }
            }

            result.Add(new AssetStats(date, aggregated));
        }

        return result;
    }

    async Task IAssetUsageTracker.TrackAsync(DomainId appId, DateOnly date, long fileSize, long count,
        CancellationToken ct)
    {
        var counters = new Counters
        {
            [AssetsKeys.TotalSize] = fileSize,
            [AssetsKeys.TotalAssets] = count
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

            tasks.Add(usageTracker.TrackAsync(date, teamKey, appId.ToString(), counters, ct));
            tasks.Add(usageTracker.TrackAsync(SummaryDate, teamKey, appId.ToString(), counters, ct));
        }

        await Task.WhenAll(tasks);
    }

    private static AssetCounters GetAssetCounters(Counters counters)
    {
        return new AssetCounters(
            counters.GetInt64(AssetsKeys.TotalSize),
            counters.GetInt64(AssetsKeys.TotalAssets));
    }

    private static string AppAssetsKey(DomainId appId)
    {
        return $"{appId}_Assets";
    }

    private static string TeamAssetsKey(DomainId teamId)
    {
        return $"{teamId}_TeamAssets";
    }
}
