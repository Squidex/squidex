// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Teams;

namespace Squidex.Domain.Apps.Entities.Billing;

public interface IBillingManager
{
    Task<Uri?> GetPortalLinkAsync(string userId, App app,
        CancellationToken ct = default);

    Task<Uri?> GetPortalLinkAsync(string userId, Team team,
        CancellationToken ct = default);

    Task<ReferralInfo?> GetReferralInfoAsync(string userId, App app,
        CancellationToken ct = default);

    Task<ReferralInfo?> GetReferralInfoAsync(string userId, Team team,
        CancellationToken ct = default);

    Task<Uri?> MustRedirectToPortalAsync(string userId, App app, string? planId,
        CancellationToken ct = default);

    Task<Uri?> MustRedirectToPortalAsync(string userId, Team team, string? planId,
        CancellationToken ct = default);

    Task SubscribeAsync(string userId, App app, string planId,
        CancellationToken ct = default);

    Task SubscribeAsync(string userId, Team team, string planId,
        CancellationToken ct = default);

    Task UnsubscribeAsync(string userId, App app,
        CancellationToken ct = default);

    Task UnsubscribeAsync(string userId, Team team,
        CancellationToken ct = default);
}
