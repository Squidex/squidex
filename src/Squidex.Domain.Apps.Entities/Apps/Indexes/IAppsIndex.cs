// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public interface IAppsIndex
    {
        Task<List<Guid>> GetIdsAsync();

        Task<List<IAppEntity>> GetAppsAsync();

        Task<List<IAppEntity>> GetAppsForUserAsync(string userId, PermissionSet permissions);

        Task<IAppEntity> GetAppAsync(string name);

        Task<IAppEntity> GetAppAsync(Guid appId);

        Task<string> ReserveAsync(Guid id, string name);

        Task<bool> AddAsync(string token);

        Task RemoveReservationAsync(string token);

        Task RebuildByContributorsAsync(string contributorId, HashSet<Guid> apps);

        Task RebuildAsync(Dictionary<string, Guid> apps);

        Task RebuildByContributorsAsync(Guid appId, HashSet<string> contributors);
    }
}
