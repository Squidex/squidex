// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure.Timers;
using Squidex.Log;

namespace Squidex.Infrastructure.Log
{
    public sealed class BackgroundRequestLogStore : DisposableObjectBase, IRequestLogStore
    {
        private const int Intervall = 10 * 1000;
        private const int BatchSize = 1000;
        private readonly IRequestLogRepository logRepository;
        private readonly ISemanticLog log;
        private readonly CompletionTimer timer;
        private ConcurrentQueue<Request> jobs = new ConcurrentQueue<Request>();

        public BackgroundRequestLogStore(IRequestLogRepository logRepository, ISemanticLog log)
        {
            Guard.NotNull(logRepository, nameof(logRepository));
            Guard.NotNull(log, nameof(log));

            this.logRepository = logRepository;

            this.log = log;

            timer = new CompletionTimer(Intervall, ct => TrackAsync(), Intervall);
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                timer.StopAsync().Wait();
            }
        }

        public void Next()
        {
            ThrowIfDisposed();

            timer.SkipCurrentDelay();
        }

        private async Task TrackAsync()
        {
            try
            {
                var localJobs = Interlocked.Exchange(ref jobs, new ConcurrentQueue<Request>());

                if (!localJobs.IsEmpty)
                {
                    var pages = (int)Math.Ceiling((double)localJobs.Count / BatchSize);

                    for (var i = 0; i < pages; i++)
                    {
                        await logRepository.InsertManyAsync(localJobs.Skip(i * BatchSize).Take(BatchSize));
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "TrackUsage")
                    .WriteProperty("status", "Failed"));
            }
        }

        public Task QueryAllAsync(Func<Request, Task> callback, string key, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        {
            return logRepository.QueryAllAsync(callback, key, fromDate, toDate, ct);
        }

        public Task LogAsync(Request request)
        {
            Guard.NotNull(request, nameof(request));

            jobs.Enqueue(request);

            return Task.CompletedTask;
        }
    }
}
