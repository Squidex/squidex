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
        private readonly CancellationTokenSource disposeToken = new CancellationTokenSource();
        private readonly Task runTask;
        private CancellationTokenSource delayToken;
        
        public CompletionTimer(int delay, Func<CancellationToken, Task> callback)
        {
            Guard.NotNull(callback, nameof(callback));
            Guard.GreaterThan(delay, 0, nameof(delay));

            runTask = RunInternal(delay, callback);
        }

        private async Task RunInternal(int delay, Func<CancellationToken, Task> callback)
        {
            while (!disposeToken.IsCancellationRequested)
            {
                try
                {
                    await callback(disposeToken.Token).ConfigureAwait(false);

                    delayToken = new CancellationTokenSource();

                    using (var cts = CancellationTokenSource.CreateLinkedTokenSource(disposeToken.Token, delayToken.Token))
                    {
                        await Task.Delay(delay, cts.Token).ConfigureAwait(false);
                    }
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
                disposeToken.Cancel();

                runTask.Wait();
            }
        }

        public void Trigger()
        {
            ThrowIfDisposed();

            delayToken?.Cancel();
        }
    }
}
