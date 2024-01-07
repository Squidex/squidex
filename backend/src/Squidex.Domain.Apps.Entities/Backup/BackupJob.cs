// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Backup;

public sealed class BackupJob : IJobRunner
{
    public const string TaskName = "backup";
    public const string ArgAppId = "appId";
    public const string ArgAppName = "appName";

    private readonly IBackupArchiveLocation backupArchiveLocation;
    private readonly IBackupArchiveStore backupArchiveStore;
    private readonly IBackupHandlerFactory backupHandlerFactory;
    private readonly IEventFormatter eventFormatter;
    private readonly IEventStore eventStore;
    private readonly IUserResolver userResolver;

    public string Name => TaskName;

    public int MaxJobs => 10;

    public BackupJob(
        IBackupArchiveLocation backupArchiveLocation,
        IBackupArchiveStore backupArchiveStore,
        IBackupHandlerFactory backupHandlerFactory,
        IEventFormatter eventFormatter,
        IEventStore eventStore,
        IUserResolver userResolver)
    {
        this.backupArchiveLocation = backupArchiveLocation;
        this.backupArchiveStore = backupArchiveStore;
        this.backupHandlerFactory = backupHandlerFactory;
        this.eventFormatter = eventFormatter;
        this.eventStore = eventStore;
        this.userResolver = userResolver;
    }

    public static (string, Dictionary<string, string>) BuildArgs(App app)
    {
        return (TaskName,
            new Dictionary<string, string>
            {
                [ArgAppId] = app.Id.ToString(),
                [ArgAppName] = app.Name
            });
    }

    public Task DownloadAsync(Job state, Stream stream,
        CancellationToken ct)
    {
        return backupArchiveStore.DownloadAsync(state.Id, stream, ct);
    }

    public Task CleanupAsync(Job state)
    {
        return backupArchiveStore.DeleteAsync(state.Id, default);
    }

    public async Task RunAsync(JobRun run,
        CancellationToken ct)
    {
        var appId = run.OwnerId;
        var appName = run.Job.Arguments.GetValueOrDefault(ArgAppName, "app");

        // We store the file in a the asset store and make the information available.
        run.Job.File = new JobFile($"backup-{appName}-{run.Job.Started:yyyy-MM-dd_HH-mm-ss}.zip", "application/zip");

        var handlers = backupHandlerFactory.CreateMany();

        await using var stream = backupArchiveLocation.OpenStream(run.Job.Id);

        using (var writer = await backupArchiveLocation.OpenWriterAsync(stream, ct))
        {
            await writer.WriteVersionAsync();

            var backupUsers = new UserMapping(run.Actor);
            var backupContext = new BackupContext(appId, backupUsers, writer);

            var streamFilter = StreamFilter.Prefix($"[^\\-]*-{appId}");

            await foreach (var storedEvent in eventStore.QueryAllAsync(streamFilter, ct: ct))
            {
                var @event = eventFormatter.Parse(storedEvent);

                if (@event.Payload is SquidexEvent { Actor: { } } squidexEvent)
                {
                    backupUsers.Backup(squidexEvent.Actor);
                }

                foreach (var handler in handlers)
                {
                    await handler.BackupEventAsync(@event, backupContext, ct);
                }

                writer.WriteEvent(storedEvent, ct);

                await run.LogAsync($"Total events: {writer.WrittenEvents}, assets: {writer.WrittenAttachments}", true);
            }

            foreach (var handler in handlers)
            {
                ct.ThrowIfCancellationRequested();

                await handler.BackupAsync(backupContext, ct);
            }

            foreach (var handler in handlers)
            {
                ct.ThrowIfCancellationRequested();

                await handler.CompleteBackupAsync(backupContext);
            }

            await backupUsers.StoreAsync(writer, userResolver, ct);
        }

        stream.Position = 0;

        ct.ThrowIfCancellationRequested();

        await backupArchiveStore.UploadAsync(run.Job.Id, stream, ct);
    }
}
