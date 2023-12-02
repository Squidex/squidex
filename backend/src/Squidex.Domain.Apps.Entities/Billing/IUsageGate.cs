// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Billing;

public interface IUsageGate
{
    Task<bool> IsBlockedAsync(App app, string? clientId, DateOnly date,
        CancellationToken ct = default);

    Task TrackRequestAsync(App app, string? clientId, DateOnly date, double costs, long elapsedMs, long bytes,
        CancellationToken ct = default);

    Task<(Plan Plan, string PlanId, DomainId? TeamId)> GetPlanForAppAsync(App app, bool canCache,
        CancellationToken ct = default);

    Task<(Plan Plan, string PlanId, DomainId? TeamId)> GetPlanForAppAsync(DomainId appId, bool canCache,
        CancellationToken ct = default);

    Task<(Plan Plan, string PlanId)> GetPlanForTeamAsync(Team team,
        CancellationToken ct = default);
}
