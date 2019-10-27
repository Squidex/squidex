// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;

namespace Squidex.Infrastructure.Orleans
{
    public interface ILockGrain : IGrainWithStringKey
    {
        Task<string?> AcquireLockAsync(string key);

        Task ReleaseLockAsync(string releaseToken);
    }
}
