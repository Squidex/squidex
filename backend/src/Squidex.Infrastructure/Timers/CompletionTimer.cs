﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Timers
{
    public sealed class CompletionTimer
    {
        private const int OneCallNotExecuted = 0;
        private const int OneCallExecuted = 1;
        private const int OneCallRequested = 2;
        private readonly CancellationTokenSource stopToken = new CancellationTokenSource();
        private readonly Task runTask;
        private int oneCallState;
        private CancellationTokenSource? wakeupToken;

        public CompletionTimer(int delayInMs, Func<CancellationToken, Task> callback, int initialDelay = 0)
        {
            Guard.NotNull(callback);
            Guard.GreaterThan(delayInMs, 0);

            runTask = RunInternalAsync(delayInMs, initialDelay, callback);
        }

        public Task StopAsync()
        {
            stopToken.Cancel();

            return runTask;
        }

        public void SkipCurrentDelay()
        {
            if (!stopToken.IsCancellationRequested)
            {
                Interlocked.CompareExchange(ref oneCallState, OneCallRequested, OneCallNotExecuted);

                wakeupToken?.Cancel();
            }
        }

        private async Task RunInternalAsync(int delay, int initialDelay, Func<CancellationToken, Task> callback)
        {
            try
            {
                if (initialDelay > 0)
                {
                    await WaitAsync(initialDelay).ConfigureAwait(false);
                }

                while (oneCallState == OneCallRequested || !stopToken.IsCancellationRequested)
                {
                    await callback(stopToken.Token).ConfigureAwait(false);

                    oneCallState = OneCallExecuted;

                    await WaitAsync(delay).ConfigureAwait(false);
                }
            }
            catch
            {
                return;
            }
        }

        private async Task WaitAsync(int intervall)
        {
            try
            {
                wakeupToken = new CancellationTokenSource();

                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(stopToken.Token, wakeupToken.Token))
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
