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
    public sealed class CompletionTimer : DisposableObject
    {
        private readonly CancellationTokenSource disposeCancellationTokenSource = new CancellationTokenSource();
        private readonly Task runTask;
        private CancellationTokenSource delayCancellationSource;
        
        public CompletionTimer(int delay, Func<CancellationToken, Task> callback)
        {
            Guard.NotNull(callback, nameof(callback));
            Guard.GreaterThan(delay, 0, nameof(delay));

            runTask = RunInternal(delay, callback);
        }

        private async Task RunInternal(int delay, Func<CancellationToken, Task> callback)
        {
            while (!disposeCancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    await callback(disposeCancellationTokenSource.Token).ConfigureAwait(false);

                    delayCancellationSource = new CancellationTokenSource();

                    await Task.Delay(delay, delayCancellationSource.Token).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Task in TriggerTimer has been cancelled.");
                }
            }
        }
        
        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                delayCancellationSource?.Cancel();
                disposeCancellationTokenSource.Cancel();

                runTask.Wait();
            }
        }

        public void Trigger()
        {
            ThrowIfDisposed();

            delayCancellationSource?.Cancel();
        }
    }
}
