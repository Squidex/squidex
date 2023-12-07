// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes;

public interface IAppsIndex
{
    Task<List<App>> GetAppsForUserAsync(string userId, PermissionSet permissions,
        CancellationToken ct = default);

    Task<List<App>> GetAppsForTeamAsync(DomainId teamId,
        CancellationToken ct = default);

    Task<App?> GetAppAsync(string name, bool canCache = false,
        CancellationToken ct = default);

    Task<App?> GetAppAsync(DomainId appId, bool canCache = false,
        CancellationToken ct = default);

    Task<string?> ReserveAsync(DomainId id, string name,
        CancellationToken ct = default);

    Task RemoveReservationAsync(string? token,
        CancellationToken ct = default);
}
