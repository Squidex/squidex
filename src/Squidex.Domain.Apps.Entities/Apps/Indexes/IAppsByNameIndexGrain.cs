// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public interface IAppsByNameIndexGrain : IGrainWithStringKey
    {
        Task<long> CountAsync();

        Task<bool> AddAppAsync(Guid appId, string name, bool reserve = false);

        Task RemoveAppAsync(Guid appId);

        Task RemoveReservationAsync(Guid appId, string name);

        Task RebuildAsync(Dictionary<string, Guid> apps);

        Task<List<Guid>> GetAppIdsAsync();

        Task<List<Guid>> GetAppIdsAsync(string[] names);

        Task<Guid> GetAppIdAsync(string name);
    }
}
