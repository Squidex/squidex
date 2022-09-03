// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Billing
{
    public interface IAppUsageTracker
    {
        Task<bool> IsBlockedAsync(IAppEntity app, string? clientId, DateTime date,
            CancellationToken ct = default);

        Task TrackRequestAsync(IAppEntity app, string? clientId, DateTime date, double costs, long elapsedMs, long bytes,
            CancellationToken ct = default);

        Task TrackAssetAsync(DomainId appId, DateTime date, long fileSize, long count,
            CancellationToken ct = default);

        Task DeleteAssetUsageAsync(DomainId appId,
            CancellationToken ct = default);

        Task DeleteAssetsUsageAsync(
            CancellationToken ct = default);

        Task<(Plan Plan, string PlanId, DomainId? TeamId)> GetPlanForAppAsync(IAppEntity app,
            CancellationToken ct = default);

        Task<(Plan Plan, string PlanId, DomainId? TeamId)> GetPlanForAppAsync(DomainId appId,
            CancellationToken ct = default);
    }
}
