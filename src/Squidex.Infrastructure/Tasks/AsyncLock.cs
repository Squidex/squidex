// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body

namespace Squidex.Infrastructure.Tasks
{
    public sealed class AsyncLock
    {
        private readonly SemaphoreSlim semaphore;

        public AsyncLock()
        {
            semaphore = new SemaphoreSlim(1);
        }

        public Task<IDisposable> LockAsync()
        {
            Task wait = semaphore.WaitAsync();

            if (wait.IsCompleted)
            {
                return Task.FromResult((IDisposable)new LockReleaser(this));
            }
            else
            {
                return wait.ContinueWith(x => (IDisposable)new LockReleaser(this),
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
        }

        private class LockReleaser : IDisposable
        {
            private AsyncLock target;

            internal LockReleaser(AsyncLock target)
            {
                this.target = target;
            }

            public void Dispose()
            {
                AsyncLock current = target;

                if (current == null)
                {
                    return;
                }

                target = null;

                try
                {
                    current.semaphore.Release();
                }
                catch
                {
                    // just ignore the Exception
                }
            }
        }
    }
}
