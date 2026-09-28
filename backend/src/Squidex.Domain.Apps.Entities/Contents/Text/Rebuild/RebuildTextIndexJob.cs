// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.ExceptionServices;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

public sealed class RebuildTextIndexJob(
    IAppProvider appProvider,
    IContentRepository contentRepository,
    ITextIndexRebuilder rebuilder,
    TextIndexRebuildCoordinator coordinator,
    TextIndexRebuildMarkers markers)
    : IJobRunner
{
    public const string TaskName = "rebuildTextIndex";
    public const string ArgAppId = "appId";
    public const string ArgAppName = "appName";
    public const string ArgSchemaId = "schemaId";
    public const string ArgSchemaName = "schemaName";

    // The rebuilder keeps the latest versions of all contents of a batch in memory.
    private const int BatchSize = 100;

    public string Name => TaskName;

    public static JobRequest BuildRequest(RefToken actor, App app, Schema? schema = null)
    {
        Guard.NotNull(actor);
        Guard.NotNull(app);

        var arguments = new Dictionary<string, string>
        {
            [ArgAppId] = app.Id.ToString(),
            [ArgAppName] = app.Name,
        };

        if (schema != null)
        {
            arguments[ArgSchemaId] = schema.Id.ToString();
            arguments[ArgSchemaName] = schema.Name;
        }

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

        DomainId? schemaId = null;

        var schemaName = context.TryGetArgument(ArgSchemaName);
        if (!string.IsNullOrWhiteSpace(schemaName))
        {
            schemaId = context.GetArgumentId(ArgSchemaId);

            // Use a readable name to describe the job.
            context.Job.Description = $"Schema {schemaName}: Rebuild full text index";
        }
        else
        {
            context.Job.Description = "Rebuild full text index";
        }

        // Mark the rebuild first, so that it is restarted when the worker crashes before the app is handed back.
        await markers.AddAsync(app.Id, schemaId, ct);

        // From now on the text indexer skips the events of the contents and remembers them for us.
        await coordinator.TakeOverAsync(app.Id, schemaId, ct);

        ExceptionDispatchInfo? error = null;
        try
        {
            await RebuildAllAsync(context, app.Id, schemaId, ct);
        }
        catch (Exception ex)
        {
            error = ExceptionDispatchInfo.Capture(ex);
        }

        // Hand the app back to the text indexer, also when the job has been cancelled or has failed.
        await HandBackAsync(context, app.Id);

        error?.Throw();
    }

    private async Task RebuildAllAsync(JobRunContext context, DomainId appId, DomainId? schemaId,
        CancellationToken ct)
    {
        HashSet<DomainId>? schemaIds = schemaId != null ? [schemaId.Value] : null;

        var batch = new List<DomainId>(BatchSize);
        var totalRebuilt = 0;

        async Task RebuildBatchAsync()
        {
            await rebuilder.RebuildAsync(appId, batch.ToArray(), ct);

            totalRebuilt += batch.Count;
            batch.Clear();

            // Also rebuild the contents that have been changed in the meantime, so that they are updated soon.
            await RebuildSkippedAsync(appId, await coordinator.TakeSkippedAsync(appId, ct), ct);

            await context.LogAsync($"Rebuilt contents: {totalRebuilt}", true);
        }

        await context.LogAsync("Started rebuild");

        // Deleted contents have already been removed from the index by the normal indexing.
        await foreach (var id in contentRepository.StreamIds(appId, schemaIds, SearchScope.All, ct))
        {
            batch.Add(id);

            if (batch.Count >= BatchSize)
            {
                await RebuildBatchAsync();
            }
        }

        if (batch.Count > 0)
        {
            await RebuildBatchAsync();
        }

        await context.LogAsync($"Completed rebuild of {totalRebuilt} contents");
    }

    private async Task HandBackAsync(JobRunContext context, DomainId appId)
    {
        try
        {
            // The handback is not cancelled with the job, but it only rebuilds the contents that have been changed in the meantime.
            while (true)
            {
                var skipped = await coordinator.TryHandBackAsync(appId);
                if (skipped.Count == 0)
                {
                    break;
                }

                await RebuildSkippedAsync(appId, skipped, default);
            }
        }
        catch
        {
            // The text indexer must not skip the app forever. The marker restarts the rebuild for the lost contents.
            await coordinator.ReleaseAsync(appId);
            throw;
        }

        await markers.RemoveAsync(appId);

        await context.LogAsync("Handed back to the text indexer");
    }

    private async Task RebuildSkippedAsync(DomainId appId, List<DomainId> contentIds,
        CancellationToken ct)
    {
        foreach (var chunk in contentIds.Chunk(BatchSize))
        {
            await rebuilder.RebuildAsync(appId, chunk, ct);
        }
    }
}
