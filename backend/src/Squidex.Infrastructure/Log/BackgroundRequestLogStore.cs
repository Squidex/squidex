// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public bool ForceWrite { get; set; }

        public bool IsEnabled => options.StoreEnabled;

        public BackgroundRequestLogStore(IOptions<RequestLogStoreOptions> options,
            IRequestLogRepository logRepository, ISemanticLog log)
        {
            this.options = options.Value;

            this.logRepository = logRepository;

            timer = new CompletionTimer(options.Value.WriteIntervall, TrackAsync, options.Value.WriteIntervall);

            this.log = log;
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

        private async Task TrackAsync(
            CancellationToken ct)
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
                        var batch = localJobs.Skip(i * batchSize).Take(batchSize);

                        if (ForceWrite)
                        {
                            ct = default;
                        }

                        await logRepository.InsertManyAsync(batch, ct);
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

        public Task DeleteAsync(string key,
            CancellationToken ct = default)
        {
            return logRepository.DeleteAsync(key, ct);
        }

        public IAsyncEnumerable<Request> QueryAllAsync(string key, DateTime fromDate, DateTime toDate,
            CancellationToken ct = default)
        {
            if (!IsEnabled)
            {
                return AsyncEnumerable.Empty<Request>();
            }

            return logRepository.QueryAllAsync(key, fromDate, toDate, ct);
        }

        public Task LogAsync(Request request,
            CancellationToken ct = default)
        {
            Guard.NotNull(request, nameof(request));

            if (!IsEnabled)
            {
                return Task.CompletedTask;
            }

            jobs.Enqueue(request);

            return Task.CompletedTask;
        }
    }
}
