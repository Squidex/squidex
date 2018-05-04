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

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppsByUserIndex : IGrainWithStringKey
    {
        Task AddAppAsync(Guid appId);

        Task RemoveAppAsync(Guid appId);

        Task RebuildAsync(HashSet<Guid> apps);

        Task<List<Guid>> GetAppIdsAsync();
    }
}
