// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Infrastructure.Log
{
    public sealed class LockingLogStore : ILogStore
    {
        private static readonly byte[] LockedText = Encoding.UTF8.GetBytes("Another process is currenty running, try it again later.");
        private static readonly TimeSpan LockWaitingTime = TimeSpan.FromMinutes(1);
        private readonly ILogStore inner;
        private readonly ILockGrain lockGrain;

        public LockingLogStore(ILogStore inner, IGrainFactory grainFactory)
        {
            Guard.NotNull(inner, nameof(inner));
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.inner = inner;

            lockGrain = grainFactory.GetGrain<ILockGrain>(SingleGrain.Id);
        }

        public Task ReadLogAsync(string key, DateTime from, DateTime to, Stream stream)
        {
            return ReadLogAsync(key, from, to, stream, LockWaitingTime);
        }

        public async Task ReadLogAsync(string key, DateTime from, DateTime to, Stream stream, TimeSpan lockTimeout)
        {
            using (var cts = new CancellationTokenSource(lockTimeout))
            {
                string releaseToken = null;

                while (!cts.IsCancellationRequested)
                {
                    releaseToken = await lockGrain.AcquireLockAsync(key);

                    if (releaseToken != null)
                    {
                        break;
                    }

                    await Task.Delay(2000, cts.Token);
                }

                if (!cts.IsCancellationRequested)
                {
                    try
                    {
                        await inner.ReadLogAsync(key, from, to, stream);
                    }
                    finally
                    {
                        await lockGrain.ReleaseLockAsync(releaseToken);
                    }
                }
                else
                {
                    await stream.WriteAsync(LockedText, 0, LockedText.Length);
                }
            }
        }
    }
}
