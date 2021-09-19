// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;

namespace Squidex.Infrastructure.Orleans.Indexes
{
    public interface IUniqueNameGrain<T> : IGrainWithStringKey
    {
        Task<string?> ReserveAsync(T id, string name);

        Task RemoveReservationAsync(string? token);
    }
}
