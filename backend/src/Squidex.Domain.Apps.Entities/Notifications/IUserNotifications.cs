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

public interface IUserNotifications
{
    bool IsActive { get; }

    Task SendUsageAsync(IUser user, IAppEntity app, long usage, long usageLimit,
        CancellationToken ct = default);

    Task SendInviteAsync(IUser assigner, IUser user, IAppEntity app,
        CancellationToken ct = default);

    Task SendInviteAsync(IUser assigner, IUser user, ITeamEntity team,
        CancellationToken ct = default);
}
