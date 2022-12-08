// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Notifications;

public sealed class NoopUserNotifications : IUserNotifications
{
    public bool IsActive
    {
        get => false;
    }

    public Task SendInviteAsync(IUser assigner, IUser user, IAppEntity app,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task SendInviteAsync(IUser assigner, IUser user, ITeamEntity team,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task SendUsageAsync(IUser user, IAppEntity app, long usage, long limit,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
