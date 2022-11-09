// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Teams;

namespace Squidex.Domain.Apps.Entities.Billing;

public interface IBillingManager
{
    Task<Uri?> GetPortalLinkAsync(string userId, IAppEntity app,
        CancellationToken ct = default);

    Task<Uri?> GetPortalLinkAsync(string userId, ITeamEntity team,
        CancellationToken ct = default);

    Task<ReferralInfo?> GetReferralInfoAsync(string userId, IAppEntity app,
        CancellationToken ct = default);

    Task<ReferralInfo?> GetReferralInfoAsync(string userId, ITeamEntity team,
        CancellationToken ct = default);

    Task<Uri?> MustRedirectToPortalAsync(string userId, IAppEntity app, string? planId,
        CancellationToken ct = default);

    Task<Uri?> MustRedirectToPortalAsync(string userId, ITeamEntity team, string? planId,
        CancellationToken ct = default);

    Task SubscribeAsync(string userId, IAppEntity app, string planId,
        CancellationToken ct = default);

    Task SubscribeAsync(string userId, ITeamEntity team, string planId,
        CancellationToken ct = default);

    Task UnsubscribeAsync(string userId, IAppEntity app,
        CancellationToken ct = default);

    Task UnsubscribeAsync(string userId, ITeamEntity team,
        CancellationToken ct = default);
}
