// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

public sealed class RebuildTextIndexJob(
    IAppProvider appProvider,
    IEventStore eventStore,
    IEventFormatter eventFormatter,
    TextIndexingProcess textIndexer,
    TextIndexRebuildCoordinator coordinator)
    : IJobRunner
{
    public const string TaskName = "rebuildTextIndex";
    public const string ArgAppId = "appId";
    public const string ArgAppName = "appName";

    // Same as the text indexer, the commands only keep the latest version of each content in the batch.
    private const int BatchSize = 1000;

    private sealed class Progress
    {
        // The position of the last event that has been written to the index.
        public string? Position { get; set; }

        public long ProcessedEvents { get; set; }
    }

    public string Name => TaskName;

    public static JobRequest BuildRequest(RefToken actor, App app)
    {
        Guard.NotNull(actor);
        Guard.NotNull(app);

        var arguments = new Dictionary<string, string>
        {
            [ArgAppId] = app.Id.ToString(),
            [ArgAppName] = app.Name,
        };

        return JobRequest.Create(actor, TaskName, arguments) with
        {
            AppId = app.NamedId(),
        };
    }

    public async Task RunAsync(JobRunContext context,
        CancellationToken ct)
    {
        var app = await appProvider.GetAppAsync(context.OwnerId, true, ct)
            ?? throw new DomainObjectNotFoundException(context.OwnerId.ToString());

        // Use a readable name to describe the job.
        context.Job.Description = "Rebuild full text index";

        // From now on the text indexer skips the events of the app and only counts them.
        await coordinator.TakeOverAsync(app.Id, ct);

        await context.LogAsync("Started rebuild");

        var progress = new Progress();

        try
        {
            await ReplayAsync(context, app.Id, progress, ct);
        }
        finally
        {
            // Hand the app back to the text indexer, also when the job has been cancelled or has failed.
            await HandBackAsync(context, app.Id, progress);
        }

        await context.LogAsync($"Completed rebuild of {progress.ProcessedEvents} events");
    }

    private async Task HandBackAsync(JobRunContext context, DomainId appId, Progress progress)
    {
        try
        {
            // The events are replayed in order, therefore the contents could be at an older version when the job has been cancelled.
            // So we cannot cancel the handback, but have to replay all remaining events.
            while (true)
            {
                var skippedEvents = await coordinator.GetSkippedEventsAsync(appId);

                await ReplayAsync(context, appId, progress, default);

                // Only hand back if the text indexer has not skipped any event while we were reading the event store.
                if (await coordinator.TryHandBackAsync(appId, skippedEvents))
                {
                    break;
                }
            }
        }
        catch
        {
            // The text indexer must not skip the app forever. The job fails and has to be started again.
            await coordinator.ReleaseAsync(appId);
            throw;
        }
    }

    private async Task ReplayAsync(JobRunContext context, DomainId appId, Progress progress,
        CancellationToken ct)
    {
        var streamFilter = StreamFilter.Prefix($"content-{appId}{DomainId.IdSeparator}");

        // Continue from the last position, e.g. to catch up with the events that have been skipped by the text indexer.
        await foreach (var batch in eventStore.QueryAllAsync(streamFilter, progress.Position, ct: ct).Batch(BatchSize, ct))
        {
            // The text indexer skips the events of the app, therefore we can replay the events without the version check.
            await textIndexer.ApplyAsync(batch.Select(eventFormatter.ParseIfKnown).NotNull().ToList(), true, ct);

            progress.Position = batch[^1].EventPosition;
            progress.ProcessedEvents += batch.Count;

            await context.LogAsync($"Rebuilt events: {progress.ProcessedEvents}", true);
        }
    }
}
