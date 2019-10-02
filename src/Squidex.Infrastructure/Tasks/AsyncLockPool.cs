// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Tasks
{
    public sealed class AsyncLockPool
    {
        private readonly AsyncLock[] locks;

        public AsyncLockPool(int poolSize)
        {
            Guard.GreaterThan(poolSize, 0);

            locks = new AsyncLock[poolSize];

            for (var i = 0; i < poolSize; i++)
            {
                locks[i] = new AsyncLock();
            }
        }

        public Task<IDisposable> LockAsync(object target)
        {
            Guard.NotNull(target);

            return locks[Math.Abs(target.GetHashCode() % locks.Length)].LockAsync();
        }
    }
}
