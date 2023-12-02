// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Collaboration;

public interface IUserNotifications
{
    bool IsActive { get; }

    Task SendUsageAsync(IUser user, App app, long usage, long usageLimit,
        CancellationToken ct = default);

    Task SendInviteAsync(IUser assigner, IUser user, App app,
        CancellationToken ct = default);

    Task SendInviteAsync(IUser assigner, IUser user, Team team,
        CancellationToken ct = default);
}
