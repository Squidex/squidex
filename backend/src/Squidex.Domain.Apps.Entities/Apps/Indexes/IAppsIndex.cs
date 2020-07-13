// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public interface IAppsIndex
    {
        Task<List<DomainId>> GetIdsAsync();

        Task<List<IAppEntity>> GetAppsAsync();

        Task<List<IAppEntity>> GetAppsForUserAsync(string userId, PermissionSet permissions);

        Task<IAppEntity?> GetAppByNameAsync(string name, bool canCache);

        Task<IAppEntity?> GetAppAsync(DomainId appId, bool canCache);

        Task<string?> ReserveAsync(DomainId id, string name);

        Task<bool> AddAsync(string? token);

        Task RemoveReservationAsync(string? token);

        Task RebuildByContributorsAsync(string contributorId, HashSet<DomainId> apps);

        Task RebuildAsync(Dictionary<string, DomainId> apps);

        Task RebuildByContributorsAsync(DomainId appId, HashSet<string> contributors);
    }
}
