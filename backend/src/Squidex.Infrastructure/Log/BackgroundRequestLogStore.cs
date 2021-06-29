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
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Timers;
using Squidex.Log;

namespace Squidex.Infrastructure.Log
{
    public sealed class BackgroundRequestLogStore : DisposableObjectBase, IRequestLogStore
    {
        private readonly IRequestLogRepository logRepository;
        private readonly ISemanticLog log;
        private readonly CompletionTimer timer;
        private readonly RequestLogStoreOptions options;
        private ConcurrentQueue<Request> jobs = new ConcurrentQueue<Request>();

        public bool IsEnabled
        {
            get => options.StoreEnabled;
        }

        public BackgroundRequestLogStore(IOptions<RequestLogStoreOptions> options,
            IRequestLogRepository logRepository, ISemanticLog log)
        {
            this.options = options.Value;

            this.logRepository = logRepository;
            this.log = log;

            timer = new CompletionTimer(options.Value.WriteIntervall, ct => TrackAsync(), options.Value.WriteIntervall);
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
            if (!IsEnabled)
            {
                return;
            }

            try
            {
                var batchSize = options.BatchSize;

                var localJobs = Interlocked.Exchange(ref jobs, new ConcurrentQueue<Request>());

                if (!localJobs.IsEmpty)
                {
                    var pages = (int)Math.Ceiling((double)localJobs.Count / batchSize);

                    for (var i = 0; i < pages; i++)
                    {
                        await logRepository.InsertManyAsync(localJobs.Skip(i * batchSize).Take(batchSize));
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
