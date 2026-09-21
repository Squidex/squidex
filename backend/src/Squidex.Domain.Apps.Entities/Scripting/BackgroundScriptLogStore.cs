// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Scripting.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Timers;

namespace Squidex.Domain.Apps.Entities.Scripting;

public sealed class BackgroundScriptLogStore : DisposableObjectBase, IScriptLogStore, IDeleter
{
    private readonly IScriptLogRepository repository;
    private readonly ILogger<BackgroundScriptLogStore> log;
    private readonly CompletionTimer timer;
    private readonly ScriptLogOptions options;
    private readonly ConcurrentQueue<ScriptLogRecord> jobs = new ConcurrentQueue<ScriptLogRecord>();
    private Instant lastCleanup;
    private int jobsCount;
    private int jobsDropped;
    private bool isUpdating;

    public IClock Clock { get; set; } = SystemClock.Instance;

    public bool HasPendingJobs => !jobs.IsEmpty || isUpdating;

    public BackgroundScriptLogStore(IOptions<ScriptLogOptions> options,
        IScriptLogRepository repository, ILogger<BackgroundScriptLogStore> log)
    {
        this.options = options.Value;
        this.repository = repository;
        this.timer = new CompletionTimer(options.Value.WriteIntervall, TrackAsync, options.Value.WriteIntervall);
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

    Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        return repository.DeleteAsync(app.Id, ct);
    }

    public void Log(DomainId appId, string name, ScriptLog log)
    {
        Guard.NotNull(log);
        Guard.NotNullOrEmpty(name);

        if (!options.Enabled || !log.HasEntries)
        {
            return;
        }

        // The queue is only drained by a timer. If the repository is not available for a longer time
        // the queue would grow until the process runs out of memory, so new entries are dropped.
        if (Volatile.Read(ref jobsCount) >= options.MaxPendingItems)
        {
            Interlocked.Increment(ref jobsDropped);
            return;
        }

        var (entries, totalEntries) = log.Snapshot();

        Interlocked.Increment(ref jobsCount);

        jobs.Enqueue(new ScriptLogRecord
        {
            AppId = appId,
            Name = name,
            Entries = entries.ToList(),
            TotalEntries = totalEntries,
            Timestamp = Clock.GetCurrentInstant(),
        });
    }

    public Task<IReadOnlyList<ScriptLogRecord>> QueryAsync(DomainId appId, string? name, int skip, int take,
        CancellationToken ct = default)
    {
        if (!options.Enabled)
        {
            return Task.FromResult<IReadOnlyList<ScriptLogRecord>>([]);
        }

        // More items than the limit are never stored, therefore the query is also limited.
        return repository.QueryAsync(
            appId,
            name,
            Math.Max(0, skip),
            Math.Clamp(take, 0, options.MaxItemsPerApp),
            ct);
    }

    private async Task TrackAsync(
        CancellationToken ct)
    {
        if (!options.Enabled)
        {
            return;
        }

        isUpdating = true;
        try
        {
            await WriteAsync(ct);
            await CleanupAsync(ct);
        }
        finally
        {
            isUpdating = false;
        }
    }

    private async Task WriteAsync(
        CancellationToken ct)
    {
        if (jobs.IsEmpty)
        {
            return;
        }

        try
        {
            // Report the entries that have been dropped since the last run, so that the gap is visible.
            var dropped = Interlocked.Exchange(ref jobsDropped, 0);

            if (dropped > 0)
            {
                LogMessages.LogScriptLogsDropped(log, dropped);
            }

            var batch = new List<ScriptLogRecord>(options.BatchSize);
            var apps = new HashSet<DomainId>();

            while (jobs.TryDequeue(out var dequeued))
            {
                Interlocked.Decrement(ref jobsCount);

                batch.Add(dequeued);
                apps.Add(dequeued.AppId);

                if (batch.Count >= options.BatchSize)
                {
                    await repository.InsertManyAsync(batch.ToList(), ct);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await repository.InsertManyAsync(batch, ct);
            }

            // Only the apps that got new logs can exceed the limit.
            foreach (var appId in apps)
            {
                await repository.TrimAsync(appId, options.MaxItemsPerApp, ct);
            }
        }
        catch (Exception ex)
        {
            LogMessages.LogFailedToWriteScriptLogs(log, ex);
        }
    }

    private async Task CleanupAsync(
        CancellationToken ct)
    {
        var now = Clock.GetCurrentInstant();

        if (now - lastCleanup < Duration.FromTimeSpan(options.CleanupIntervall))
        {
            return;
        }

        lastCleanup = now;
        try
        {
            // Apps that do not log anymore are not trimmed, therefore old logs are removed separately.
            await repository.DeleteOlderThanAsync(now.Minus(Duration.FromDays(options.RetentionInDays)), ct);
        }
        catch (Exception ex)
        {
            LogMessages.LogFailedToCleanupScriptLogs(log, ex);
        }
    }
}
