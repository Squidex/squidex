// ==========================================================================
//  CompletionTimer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable InvertIf

namespace Squidex.Infrastructure.Timers
{
    public sealed class CompletionTimer : DisposableObjectBase
    {
        private readonly CancellationTokenSource disposeToken = new CancellationTokenSource();
        private readonly Task runTask;
        private int requiresAtLeastOne;
        private CancellationTokenSource wakeupToken;
        
        public CompletionTimer(int delayInMs, Func<CancellationToken, Task> callback, int initialDelay = 0)
        {
            Guard.NotNull(callback, nameof(callback));
            Guard.GreaterThan(delayInMs, 0, nameof(delayInMs));

            runTask = RunInternal(delayInMs, initialDelay, callback);
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                disposeToken.Cancel();

                runTask.Wait();
            }
        }

        public void Wakeup()
        {
            ThrowIfDisposed();

            Interlocked.CompareExchange(ref requiresAtLeastOne, 2, 0);

            wakeupToken?.Cancel();
        }

        private async Task RunInternal(int delay, int initialDelay, Func<CancellationToken, Task> callback)
        {
            if (initialDelay > 0)
            {
                await WaitAsync(initialDelay).ConfigureAwait(false);
            }

            while (requiresAtLeastOne == 2 || !disposeToken.IsCancellationRequested)
            {
                try
                {
                    await callback(disposeToken.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    requiresAtLeastOne = 1;
                }

                await WaitAsync(delay).ConfigureAwait(false);
            }
        }

        private async Task WaitAsync(int intervall)
        {
            try
            {
                wakeupToken = new CancellationTokenSource();

                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(disposeToken.Token, wakeupToken.Token))
                {
                    await Task.Delay(intervall, cts.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
