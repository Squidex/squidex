// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Timers;

namespace Squidex.Infrastructure.Log;

public sealed class BackgroundRequestLogStore : DisposableObjectBase, IRequestLogStore
{
    private readonly IRequestLogRepository logRepository;
    private readonly ILogger<BackgroundRequestLogStore> log;
    private readonly CompletionTimer logTimer;
    private readonly RequestLogStoreOptions options;
    private readonly ConcurrentQueue<Request> jobs = new ConcurrentQueue<Request>();
    private bool isUpdating;

    public bool HasPendingJobs => !jobs.IsEmpty || isUpdating;

    public bool IsEnabled => options.StoreEnabled;

    public BackgroundRequestLogStore(IOptions<RequestLogStoreOptions> options,
        IRequestLogRepository logRepository, ILogger<BackgroundRequestLogStore> log)
    {
        this.options = options.Value;

        this.logRepository = logRepository;
        this.logTimer = new CompletionTimer(options.Value.WriteIntervall, TrackAsync, options.Value.WriteIntervall);

        this.log = log;
    }

    protected override void DisposeObject(bool disposing)
    {
        if (disposing)
        {
            logTimer.StopAsync().Wait();
        }
    }

    public void Next()
    {
        ThrowIfDisposed();

        logTimer.SkipCurrentDelay();
    }

    private async Task TrackAsync(
        CancellationToken ct)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (jobs.IsEmpty)
        {
            return;
        }

        isUpdating = true;
        try
        {
            var batch = new List<Request>(options.BatchSize);

            while (jobs.TryDequeue(out var dequeued))
            {
                batch.Add(dequeued);

                if (batch.Count >= options.BatchSize)
                {
                    await logRepository.InsertManyAsync(batch.ToList(), ct);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await logRepository.InsertManyAsync(batch, ct);
            }
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to track usage in background.");
        }
        finally
        {
            isUpdating = false;
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
        Guard.NotNull(request);

        if (!IsEnabled)
        {
            return Task.CompletedTask;
        }

        jobs.Enqueue(request);

        return Task.CompletedTask;
    }
}
