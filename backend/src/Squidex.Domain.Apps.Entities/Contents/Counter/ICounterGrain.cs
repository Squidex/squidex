// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;

namespace Squidex.Domain.Apps.Entities.Contents.Counter
{
    public interface ICounterGrain : IGrainWithStringKey
    {
        Task<long> IncrementAsync(string name);

        Task<long> ResetAsync(string name, long value);

        Task ClearAsync();
    }
}
