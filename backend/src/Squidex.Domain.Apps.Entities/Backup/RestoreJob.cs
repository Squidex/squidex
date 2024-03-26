// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Translations;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Backup;

public sealed class RestoreJob : IJobRunner
{
    public const string TaskName = "restore";
    public const string ArgUrl = "url";
    public const string ArgName = "name";

    private readonly IBackupArchiveLocation backupArchiveLocation;
    private readonly IBackupHandlerFactory backupHandlerFactory;
    private readonly ICommandBus commandBus;
    private readonly IEventFormatter eventFormatter;
    private readonly IEventStore eventStore;
    private readonly IEventStreamNames eventStreamNames;
    private readonly IUserResolver userResolver;
    private readonly ILogger<RestoreJob> log;

    // Use a run to store all state that is necessary for a single run.
    private sealed class State
    {
        public NamedId<DomainId> AppId { get; set; }

        public IEnumerable<IBackupHandler> Handlers { get; init; }

        public IBackupReader Reader { get; set; }

        public RestoreContext Context { get; set; }

        public StreamMapper StreamMapper { get; set; }

        public string? NewAppName { get; init; }

        public Uri Url { get; internal set; }
    }

    public string Name => TaskName;

    public RestoreJob(
        IBackupArchiveLocation backupArchiveLocation,
        IBackupHandlerFactory backupHandlerFactory,
        ICommandBus commandBus,
        IEventFormatter eventFormatter,
        IEventStore eventStore,
        IEventStreamNames eventStreamNames,
        IUserResolver userResolver,
        ILogger<RestoreJob> log)
    {
        this.backupArchiveLocation = backupArchiveLocation;
        this.backupHandlerFactory = backupHandlerFactory;
        this.commandBus = commandBus;
        this.eventFormatter = eventFormatter;
        this.eventStore = eventStore;
        this.eventStreamNames = eventStreamNames;
        this.userResolver = userResolver;
        this.log = log;
    }

    public static JobRequest BuildRequest(RefToken actor, Uri url, string? appName)
    {
        return JobRequest.Create(
            actor,
            TaskName,
            new Dictionary<string, string>
            {
                [ArgUrl] = url.ToString(),
                [ArgName] = appName ?? string.Empty
            });
    }

    public async Task RunAsync(JobRunContext context,
        CancellationToken ct)
    {
        if (!context.Job.Arguments.TryGetValue(ArgUrl, out var urlValue) || !Uri.TryCreate(urlValue, UriKind.Absolute, out var url))
        {
            throw new DomainException("Argument missing.");
        }

        var state = new State
        {
            Handlers = backupHandlerFactory.CreateMany(),
            // Required argument.
            Url = url,
            // Optional argument.
            NewAppName = context.Job.Arguments.GetValueOrDefault(ArgName)
        };

        // Use a readable name to describe the job.
        context.Job.Description = T.Get("jobs.restore");

        try
        {
            await context.LogAsync("Started. The restore process has the following steps:");
            await context.LogAsync("  * Download backup");
            await context.LogAsync("  * Restore events and attachments.");
            await context.LogAsync("  * Restore all objects like app, schemas and contents");
            await context.LogAsync("  * Complete the restore operation for all objects");
            await context.FlushAsync();

            log.LogInformation("Backup with job id {backupId} with from URL '{url}' started.", context.Job.Id, state.Url);

            state.Reader = await DownloadAsync(context, state, ct);

            await state.Reader.CheckCompatibilityAsync();

            using (Telemetry.Activities.StartActivity("ReadEvents"))
            {
                await ReadEventsAsync(context, state, ct);
            }

            if (state.Context == null)
            {
                throw new BackupRestoreException("Backup has no event.");
            }

            foreach (var handler in state.Handlers)
            {
                using (Telemetry.Activities.StartActivity($"{handler.GetType().Name}/RestoreAsync"))
                {
                    await handler.RestoreAsync(state.Context, ct);
                }

                await context.LogAsync($"Restored {handler.Name}");
            }

            foreach (var handler in state.Handlers)
            {
                using (Telemetry.Activities.StartActivity($"{handler.GetType().Name}/CompleteRestoreAsync"))
                {
                    await handler.CompleteRestoreAsync(state.Context, state.NewAppName!);
                }

                await context.LogAsync($"Completed {handler.Name}");
            }

            // Add the current user to the app, so that the admin can see it and verify integrity.
            await AssignContributorAsync(context, state);

            await context.LogAsync("Completed, Yeah!");

            log.LogInformation("Backup with job id {backupId} from URL '{url}' completed.", context.Job.Id, state.Url);
        }
        catch (Exception ex)
        {
            // Cleanup as soon as possible.
            await CleanupAsync(state);

            var message = "Failed with internal error.";

            switch (ex)
            {
                case BackupRestoreException backupException:
                    message = backupException.Message;
                    break;
                case FileNotFoundException fileNotFoundException:
                    message = fileNotFoundException.Message;
                    break;
            }

            await context.LogAsync(message);

            log.LogError(ex, "Backup with job id {backupId} from URL '{url}' failed.", context.Job.Id, state.Url);
            throw;
        }
    }

    private async Task AssignContributorAsync(JobRunContext run, State state)
    {
        if (run.Actor?.IsUser != true)
        {
            await run.LogAsync("Current user not assigned because restore was triggered by client.");
            return;
        }

        try
        {
            // Add the current user to the app, so that the admin can see it and verify integrity.
            await PublishAsync(run, state, new AssignContributor
            {
                ContributorId = run.Actor.Identifier,
                IgnoreActor = true,
                IgnorePlans = true,
                Role = Role.Owner
            });

            await run.LogAsync("Assigned current user.");
        }
        catch (DomainException ex)
        {
            await run.LogAsync($"Failed to assign contributor: {ex.Message}");
        }
    }

    private Task<CommandContext> PublishAsync(JobRunContext run, State state, AppCommand command)
    {
        command.Actor = run.Actor;

        if (command is IAppCommand appCommand)
        {
            appCommand.AppId = state.AppId;
        }

        return commandBus.PublishAsync(command, default);
    }

    private async Task CleanupAsync(State state)
    {
        if (state.AppId == null)
        {
            return;
        }

        foreach (var handler in state.Handlers)
        {
            try
            {
                await handler.CleanupRestoreErrorAsync(state.AppId.Id);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to clean up restore.");
            }
        }
    }

    private async Task<IBackupReader> DownloadAsync(JobRunContext run, State state,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("Download"))
        {
            await run.LogAsync("Downloading Backup");

            var reader = await backupArchiveLocation.OpenReaderAsync(state.Url, run.Job.Id, ct);

            await run.LogAsync("Downloaded Backup");

            return reader;
        }
    }

    private async Task ReadEventsAsync(JobRunContext run, State state,
        CancellationToken ct)
    {
        // Run batch first, because it is cheaper as it has less items.
        var events = HandleEventsAsync(run, state, ct).Batch(100, ct).Buffered(2, ct);

        var handled = 0;

        await Parallel.ForEachAsync(events, new ParallelOptions
        {
            CancellationToken = ct,
            // The event store cannot insert events in parallel.
            MaxDegreeOfParallelism = 1,
        },
        async (batch, ct) =>
        {
            var commits =
                batch.Select(item =>
                    EventCommit.Create(
                        item.Stream,
                        item.Offset,
                        item.Event,
                        eventFormatter));

            await eventStore.AppendUnsafeAsync(commits, ct);

            // Just in case we use parallel inserts later.
            Interlocked.Add(ref handled, batch.Count);

            await run.LogAsync($"Reading {state.Reader.ReadEvents}/{handled} events and {state.Reader.ReadAttachments} attachments completed.", true);
        });
    }

    private async IAsyncEnumerable<(string Stream, long Offset, Envelope<IEvent> Event)> HandleEventsAsync(JobRunContext run, State state,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var @events = state.Reader.ReadEventsAsync(eventStreamNames, eventFormatter, ct);

        await foreach (var (stream, @event) in events.WithCancellation(ct))
        {
            var (newStream, handled) = await HandleEventAsync(run, state, stream, @event, ct);

            if (handled)
            {
                var offset = state.StreamMapper.GetStreamOffset(newStream);

                yield return (newStream, offset, @event);
            }
        }
    }

    private async Task<(string StreamName, bool Handled)> HandleEventAsync(JobRunContext run, State state, string stream, Envelope<IEvent> @event,
        CancellationToken ct = default)
    {
        if (@event.Payload is AppCreated appCreated)
        {
            var previousAppId = appCreated.AppId.Id;

            if (!string.IsNullOrWhiteSpace(state.NewAppName))
            {
                appCreated.Name = state.NewAppName;

                state.AppId = NamedId.Of(DomainId.NewGuid(), state.NewAppName);
            }
            else
            {
                state.AppId = NamedId.Of(DomainId.NewGuid(), appCreated.Name);
            }

            await CreateContextAsync(run, state, previousAppId, ct);

            state.StreamMapper = new StreamMapper(state.Context);
        }

        if (@event.Payload is SquidexEvent { Actor: { } } squidexEvent)
        {
            if (state.Context.UserMapping.TryMap(squidexEvent.Actor, out var newUser))
            {
                squidexEvent.Actor = newUser;
            }
        }

        if (@event.Payload is AppEvent appEvent)
        {
            appEvent.AppId = state.AppId;
        }

        var (newStream, id) = state.StreamMapper.Map(stream);

        @event.SetAggregateId(id);
        @event.SetRestored();

        foreach (var handler in state.Handlers)
        {
            if (!await handler.RestoreEventAsync(@event, state.Context, ct))
            {
                return (newStream, false);
            }
        }

        return (newStream, true);
    }

    private async Task CreateContextAsync(JobRunContext run, State state, DomainId previousAppId,
        CancellationToken ct)
    {
        var userMapping = new UserMapping(run.Actor);

        using (Telemetry.Activities.StartActivity("CreateUsers"))
        {
            await run.LogAsync("Creating Users");

            await userMapping.RestoreAsync(state.Reader, userResolver, ct);

            await run.LogAsync("Created Users");
        }

        state.Context = new RestoreContext(state.AppId.Id, userMapping, state.Reader, previousAppId);
    }
}
