// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Teams;

namespace Squidex.Domain.Apps.Entities.Billing;

public sealed class NoopBillingManager : IBillingManager
{
    public Task<Uri?> GetPortalLinkAsync(string userId, App app,
        CancellationToken ct = default)
    {
        return Task.FromResult<Uri?>(null);
    }

    public Task<Uri?> GetPortalLinkAsync(string userId, Team team,
        CancellationToken ct = default)
    {
        return Task.FromResult<Uri?>(null);
    }

    public Task<ReferralInfo?> GetReferralInfoAsync(string userId, App app,
        CancellationToken ct = default)
    {
        return Task.FromResult<ReferralInfo?>(null);
    }

    public Task<ReferralInfo?> GetReferralInfoAsync(string userId, Team team,
        CancellationToken ct = default)
    {
        return Task.FromResult<ReferralInfo?>(null);
    }

    public Task<Uri?> MustRedirectToPortalAsync(string userId, App app, string? planId,
        CancellationToken ct = default)
    {
        return Task.FromResult<Uri?>(null);
    }

    public Task<Uri?> MustRedirectToPortalAsync(string userId, Team team, string? planId,
        CancellationToken ct = default)
    {
        return Task.FromResult<Uri?>(null);
    }

    public Task SubscribeAsync(string userId, App app, string planId,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task SubscribeAsync(string userId, Team team, string planId,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(string userId, App app,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(string userId, Team team,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
