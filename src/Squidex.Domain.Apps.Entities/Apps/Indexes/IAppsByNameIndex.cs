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
    public interface IAppsByNameIndex : IGrainWithStringKey
    {
        Task<bool> ReserveAppAsync(Guid appId, string name);

        Task AddAppAsync(Guid appId, string name);

        Task RemoveAppAsync(Guid appId);

        Task RebuildAsync(Dictionary<string, Guid> apps);

        Task RemoveReservationAsync(Guid appId, string name);

        Task<Guid> GetAppIdAsync(string name);

        Task<List<Guid>> GetAppIdsAsync();
    }
}
