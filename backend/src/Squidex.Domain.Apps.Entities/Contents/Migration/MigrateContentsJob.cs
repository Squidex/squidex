// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Migration;

public sealed class MigrateContentsJob(
    IAppProvider appProvider,
    ICommandBus commandBus,
    IContentRepository contentRepository,
    IContextProvider contextProvider,
    IJsonSerializer jsonSerializer)
    : IJobRunner
{
    public const string TaskName = "migrateContents";
    public const string ArgAppId = "appId";
    public const string ArgAppName = "appName";
    public const string ArgSchemaId = "schemaId";
    public const string ArgSchemaName = "schemaName";

    // Schema changes are not applied to the stored contents, therefore a lot of items can be affected.
    private const int BatchSize = 100;

    public string Name => TaskName;

    public static JobRequest BuildRequest(RefToken actor, App app, Schema schema)
    {
        Guard.NotNull(actor);
        Guard.NotNull(app);
        Guard.NotNull(schema);

        return JobRequest.Create(
            actor,
            TaskName,
            new Dictionary<string, string>
            {
                [ArgAppId] = app.Id.ToString(),
                [ArgAppName] = app.Name,
                [ArgSchemaId] = schema.Id.ToString(),
                [ArgSchemaName] = schema.Name,
            }) with
        {
            AppId = app.NamedId(),
        };
    }

    public async Task RunAsync(JobRunContext context,
        CancellationToken ct)
    {
        // The other arguments are just there for debugging purposes. Therefore do not validate them.
        if (!context.Job.Arguments.TryGetValue(ArgSchemaId, out var schemaIdValue))
        {
            throw new DomainException($"Argument '{ArgSchemaId}' missing.");
        }

        if (!context.Job.Arguments.TryGetValue(ArgSchemaName, out var schemaName))
        {
            throw new DomainException($"Argument '{ArgSchemaName}' missing.");
        }

        var schemaId = DomainId.Create(schemaIdValue);

        var (app, schema) = await appProvider.GetAppWithSchemaAsync(context.OwnerId, schemaId, ct: ct);
        if (app == null)
        {
            throw new DomainObjectNotFoundException(context.OwnerId.ToString());
        }

        if (schema == null)
        {
            throw new DomainObjectNotFoundException(schemaIdValue);
        }

        // Use a readable name to describe the job.
        context.Job.Description = $"Schema {schemaName}: Migrate contents";

        // The job is not running in the scope of a request, therefore we have to provide the context for the commands.
        contextProvider.Context = Context.Admin(app).Clone(b => b
            .WithNoEnrichment()
            .WithNoCleanup()
            .WithUnpublished(true));

        var components = await appProvider.GetComponentsAsync(schema, ct);
        var converter = BuildConverter(app, schema, components);

        // Buffer the batches, so that the next contents are read while the current batch is written.
        var batches =
            contentRepository.StreamAll(app.Id, [schemaId], SearchScope.All, ct)
                .Batch(BatchSize, ct)
                .Buffered(2, ct);

        var totalCount = 0;
        var totalUpdates = 0;

        await foreach (var batch in batches)
        {
            var jobs = new List<BulkUpdateJob>(batch.Count);

            foreach (var content in batch)
            {
                // The converter takes ownership of the data, therefore the clone is needed for the comparison.
                var converted = converter.Convert(content.Data.Clone());
                if (converted.Equals(content.Data))
                {
                    continue;
                }

                jobs.Add(new BulkUpdateJob
                {
                    Id = content.Id,
                    Data = converted,
                    Type = BulkUpdateContentType.Update,
                });
            }

            totalCount += batch.Count;
            totalUpdates += jobs.Count;

            if (jobs.Count > 0)
            {
                await UpdateAsync(context, app, schema, jobs, ct);
            }

            await context.LogAsync($"Checked contents: {totalCount}, updated: {totalUpdates}", true);
        }

        await context.LogAsync($"Checked contents: {totalCount}, updated: {totalUpdates}", true);
    }

    private ContentConverter BuildConverter(App app, Schema schema, ResolvedComponents components)
    {
        // The converter itself removes all fields and components that are not part of the schema anymore.
        var converter = new ContentConverter(components, schema);

        // Remove all values that are not compatible with the current field type.
        converter.Add(new ExcludeChangedTypes(jsonSerializer));

        // Move the values over when the partitioning of a field has been changed.
        converter.Add(new ResolveFromPreviousPartitioning(app.Languages));

        return converter;
    }

    private async Task UpdateAsync(JobRunContext context, App app, Schema schema, List<BulkUpdateJob> jobs,
        CancellationToken ct)
    {
        // The contents are only converted to the current schema, therefore all custom logic must be skipped.
        var command = new BulkUpdateContents
        {
            Jobs = jobs.ToArray(),
            Actor = context.Actor,
            AppId = app.NamedId(),
            DoNotScript = true,
            DoNotValidate = true,
            DoNotValidateWorkflow = true,
            OptimizeValidation = true,
            SchemaId = schema.NamedId(),
        };

        var commandContext = await commandBus.PublishAsync(command, ct);

        // Errors are reported per content item, so that a single invalid item does not stop the migration.
        if (commandContext.PlainResult is not BulkUpdateResult result)
        {
            return;
        }

        foreach (var item in result)
        {
            if (item.Exception != null)
            {
                await context.LogAsync($"Failed to migrate content {item.Id}: {item.Exception.Message}");
            }
        }
    }
}
