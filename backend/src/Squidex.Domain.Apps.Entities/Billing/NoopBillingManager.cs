// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Teams;

namespace Squidex.Domain.Apps.Entities.Billing;

public sealed class NoopBillingManager : IBillingManager
{
    public Task<Uri?> GetPortalLinkAsync(string userId, IAppEntity app,
        CancellationToken ct = default)
    {
        return Task.FromResult<Uri?>(null);
    }

    public Task<Uri?> GetPortalLinkAsync(string userId, ITeamEntity team,
        CancellationToken ct = default)
    {
        return Task.FromResult<Uri?>(null);
    }

    public Task<ReferralInfo?> GetReferralInfoAsync(string userId, IAppEntity app,
        CancellationToken ct = default)
    {
        return Task.FromResult<ReferralInfo?>(null);
    }

    public Task<ReferralInfo?> GetReferralInfoAsync(string userId, ITeamEntity team,
        CancellationToken ct = default)
    {
        return Task.FromResult<ReferralInfo?>(null);
    }

    public Task<Uri?> MustRedirectToPortalAsync(string userId, IAppEntity app, string? planId,
        CancellationToken ct = default)
    {
        return Task.FromResult<Uri?>(null);
    }

    public Task<Uri?> MustRedirectToPortalAsync(string userId, ITeamEntity team, string? planId,
        CancellationToken ct = default)
    {
        return Task.FromResult<Uri?>(null);
    }

    public Task SubscribeAsync(string userId, IAppEntity app, string planId,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task SubscribeAsync(string userId, ITeamEntity team, string planId,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(string userId, IAppEntity app,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(string userId, ITeamEntity team,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
