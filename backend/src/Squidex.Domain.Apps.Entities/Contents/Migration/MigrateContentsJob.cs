// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Queries;
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
    public const string ArgMigrateDraft = "migrateDraft";
    public const string ArgMigratePublished = "migratePublished";

    private const int BatchSize = 100;

    public string Name => TaskName;

    public static JobRequest BuildRequest(RefToken actor, App app, Schema schema, bool migrateDraft = true, bool migratePublished = true)
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
                [ArgMigrateDraft] = migrateDraft.ToString(),
                [ArgMigratePublished] = migratePublished.ToString(),
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

        var migrateDraft = GetFlag(context.Job, ArgMigrateDraft);
        var migratePublished = GetFlag(context.Job, ArgMigratePublished);

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
        var converter = ContentMigration.CreateConverter(schema, components, app.Languages, jsonSerializer);

        // Buffer the batches, so that the next contents are read while the current batch is written.
        var batches =
            contentRepository.StreamAll(app.Id, [schemaId], SearchScope.All, ct)
                .Batch(BatchSize, ct)
                .Buffered(2, ct);

        var totalCount = 0;
        var totalSubmitted = 0;

        // The progress is a single log line, but it must not overwrite the errors of the previous batch.
        var replaceProgress = false;

        await foreach (var batch in batches)
        {
            var publishedData = await GetPublishedDataAsync(app, schema, batch, migratePublished, ct);

            var jobs = new List<BulkUpdateJob>(batch.Count);

            foreach (var content in batch)
            {
                ContentData? newCurrentData;
                ContentData? newDraftData = null;

                if (content.NewStatus != null)
                {
                    // The stream only contains the draft of the content, the published version is queried separately.
                    newCurrentData = publishedData.GetValueOrDefault(content.Id) is ContentData data ? Convert(converter, data) : null;
                    newDraftData = migrateDraft ? Convert(converter, content.Data) : null;
                }
                else
                {
                    // The current version is only the published version, if it has the published status. Otherwise it is a draft.
                    var migrateCurrent = content.Status == Status.Published ? migratePublished : migrateDraft;

                    newCurrentData = migrateCurrent ? Convert(converter, content.Data) : null;
                }

                if (newCurrentData == null && newDraftData == null)
                {
                    continue;
                }

                // Fail the item instead of overwriting a change that has been made after the content has been read.
                jobs.Add(new BulkUpdateJob
                {
                    Id = content.Id,
                    Data = newCurrentData,
                    ExpectedCount = 1,
                    ExpectedVersion = content.Version,
                    NewData = newDraftData,
                    Type = BulkUpdateContentType.Migrate,
                });
            }

            totalCount += batch.Count;
            totalSubmitted += jobs.Count;

            var errors = 0;

            if (jobs.Count > 0)
            {
                errors = await MigrateAsync(context, app, schema, jobs, ct);
            }

            await context.LogAsync($"Checked contents: {totalCount}, submitted: {totalSubmitted}", replaceProgress && errors == 0);
            replaceProgress = true;
        }

        await context.LogAsync($"Checked contents: {totalCount}, submitted: {totalSubmitted}", replaceProgress);
    }

    private async Task<Dictionary<DomainId, ContentData>> GetPublishedDataAsync(App app, Schema schema, List<Content> batch, bool migratePublished,
        CancellationToken ct)
    {
        var result = new Dictionary<DomainId, ContentData>();

        if (!migratePublished)
        {
            return result;
        }

        var ids = batch.Where(x => x.NewStatus != null).Select(x => x.Id).ToHashSet();
        if (ids.Count == 0)
        {
            return result;
        }

        var published = await contentRepository.QueryAsync(app, schema, Q.Empty.WithIds(ids).WithoutTotal(), SearchScope.Published, ct);

        foreach (var content in published)
        {
            result[content.Id] = content.Data;
        }

        return result;
    }

    private static ContentData? Convert(ContentConverter converter, ContentData data)
    {
        // The converter takes ownership of the data, therefore the clone is needed for the comparison.
        var converted = converter.Convert(data.Clone());

        return converted.Equals(data) ? null : converted;
    }

    private static bool GetFlag(Job job, string name)
    {
        // Migrate all versions by default, if the argument has not been provided.
        return !job.Arguments.TryGetValue(name, out var value) || !bool.TryParse(value, out var result) || result;
    }

    private async Task<int> MigrateAsync(JobRunContext context, App app, Schema schema, List<BulkUpdateJob> jobs,
        CancellationToken ct)
    {
        var command = new BulkUpdateContents
        {
            Actor = context.Actor,
            AppId = app.NamedId(),
            Jobs = jobs.ToArray(),
            SchemaId = schema.NamedId(),
        };

        var commandContext = await commandBus.PublishAsync(command, ct);

        // Errors are reported per content item, so that a single invalid item does not stop the migration.
        if (commandContext.PlainResult is not BulkUpdateResult result)
        {
            return 0;
        }

        var errors = 0;

        foreach (var item in result)
        {
            if (item.Exception != null)
            {
                await context.LogAsync($"Failed to migrate content {item.Id}: {item.Exception.Message}");
                errors++;
            }
        }

        return errors;
    }
}
