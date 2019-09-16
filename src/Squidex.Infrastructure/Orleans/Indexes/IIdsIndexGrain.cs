// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Orleans.Indexes
{
    public interface IIdsIndexGrain<T>
    {
        Task<long> CountAsync();

        Task RebuildAsync(HashSet<T> ids);

        Task AddAsync(T id);

        Task RemoveAsync(T id);

        Task ClearAsync();

        Task<List<T>> GetIdsAsync();
    }
}
