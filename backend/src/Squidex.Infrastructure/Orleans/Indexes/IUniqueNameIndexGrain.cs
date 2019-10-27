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
    public interface IUniqueNameIndexGrain<T>
    {
        Task<string?> ReserveAsync(T id, string name);

        Task<bool> AddAsync(string? token);

        Task<long> CountAsync();

        Task RemoveReservationAsync(string? token);

        Task RemoveAsync(T id);

        Task RebuildAsync(Dictionary<string, T> values);

        Task ClearAsync();

        Task<T> GetIdAsync(string name);

        Task<List<T>> GetIdsAsync(string[] names);

        Task<List<T>> GetIdsAsync();
    }
}
