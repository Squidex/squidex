// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class RebuildTextIndexJob(
    IAppProvider appProvider,
    IContentRepository contentRepository,
    ITextIndexRebuilder textIndexRebuilder)
    : IJobRunner
{
    public const string TaskName = "rebuildTextIndex";
    public const string ArgAppId = "appId";
    public const string ArgAppName = "appName";
    public const string ArgSchemaId = "schemaId";
    public const string ArgSchemaName = "schemaName";

    // Keep the batches small, because the normal indexing has to wait for each batch.
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

        HashSet<DomainId>? schemaIds = null;

        var schemaName = context.TryGetArgument(ArgSchemaName);

        if (!string.IsNullOrWhiteSpace(schemaName))
        {
            schemaIds = [context.GetArgumentId(ArgSchemaId)];

            // Use a readable name to describe the job.
            context.Job.Description = $"Schema {schemaName}: Rebuild full text index";
        }
        else
        {
            context.Job.Description = "Rebuild full text index";
        }

        var batch = new List<DomainId>(BatchSize);
        var totalRebuilt = 0;

        async Task RebuildBatchAsync()
        {
            await textIndexRebuilder.RebuildAsync(app.Id, batch.ToArray(), ct);

            totalRebuilt += batch.Count;
            batch.Clear();

            await context.LogAsync($"Rebuilt contents: {totalRebuilt}", true);
        }

        await context.LogAsync("Started rebuild");

        // Deleted contents have already been removed from the index by the normal indexing.
        await foreach (var id in contentRepository.StreamIds(app.Id, schemaIds, SearchScope.All, ct))
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
}
