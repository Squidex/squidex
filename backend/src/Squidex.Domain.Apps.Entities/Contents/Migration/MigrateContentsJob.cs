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
    private sealed class MigrationContext
    {
        required public JobRunContext Run { get; init; }

        required public App App { get; init; }

        required public Schema Schema { get; init; }

        required public ContentConverter Converter { get; init; }

        required public bool MigrateDraft { get; init; }

        required public bool MigratePublished { get; init; }
    }

    public const string TaskName = "migrateContents";
    public const string ArgAppId = "appId";
    public const string ArgAppName = "appName";
    public const string ArgSchemaId = "schemaId";
    public const string ArgSchemaName = "schemaName";
    public const string ArgMigrateDraft = "migrateDraft";
    public const string ArgMigratePublished = "migratePublished";

    private const int BatchSize = 100;
    private const int MaxRetries = 3;

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
        var schemaId = context.GetArgumentId(ArgSchemaId);
        var schemaName = context.GetArgument(ArgSchemaName);

        // Migrate all versions by default, if the arguments have not been provided.
        var migrateDraft = context.GetArgumentFlag(ArgMigrateDraft, true);
        var migratePublished = context.GetArgumentFlag(ArgMigratePublished, true);

        var (app, schema) = await appProvider.GetAppWithSchemaAsync(context.OwnerId, schemaId, ct: ct);
        if (app == null)
        {
            throw new DomainObjectNotFoundException(context.OwnerId.ToString());
        }

        if (schema == null)
        {
            throw new DomainObjectNotFoundException(schemaId.ToString());
        }

        // Use a readable name to describe the job.
        context.Job.Description = $"Schema {schemaName}: Migrate contents";

        // The job is not running in the scope of a request, therefore we have to provide the context for the commands.
        contextProvider.Context = Context.Admin(app).Clone(b => b
            .WithNoEnrichment()
            .WithNoCleanup()
            .WithUnpublished(true));

        var components = await appProvider.GetComponentsAsync(schema, ct);

        await MigrateAsync(
            new MigrationContext
            {
                App = app,
                Converter = CreateConverter(app, schema, components),
                MigrateDraft = migrateDraft,
                MigratePublished = migratePublished,
                Run = context,
                Schema = schema,
            }, ct);
    }

    private async Task MigrateAsync(MigrationContext migration,
        CancellationToken ct)
    {
        // Buffer the batches, so that the next contents are read while the current batch is written.
        var batches =
            contentRepository.StreamWriteContents(migration.App.Id, [migration.Schema.Id], null, ct)
                .Batch(BatchSize, ct)
                .Buffered(2, ct);

        var totalCount = 0;
        var totalSubmitted = 0;

        // The progress is a single log line, but it must not overwrite the errors of the previous batch.
        var replaceProgress = false;

        await foreach (var batch in batches)
        {
            var (submitted, errors) = await MigrateBatchAsync(migration, batch, ct);

            totalCount += batch.Count;
            totalSubmitted += submitted;

            await migration.Run.LogAsync($"Checked contents: {totalCount}, submitted: {totalSubmitted}", replaceProgress && errors == 0);
            replaceProgress = true;
        }

        await migration.Run.LogAsync($"Checked contents: {totalCount}, submitted: {totalSubmitted}", replaceProgress);
    }

    private ContentConverter CreateConverter(App app, Schema schema, ResolvedComponents components)
    {
        // The converter itself removes all fields and components that are not part of the schema anymore.
        var converter = new ContentConverter(components, schema);

        // Remove all values that are not compatible with the current field type.
        converter.Add(new ExcludeChangedTypes(jsonSerializer));

        // Move the values over when the partitioning of a field has been changed.
        converter.Add(new ResolveFromPreviousPartitioning(app.Languages));

        return converter;
    }

    private async Task<(int Submitted, int Errors)> MigrateBatchAsync(MigrationContext migration, List<WriteContent> contents,
        CancellationToken ct)
    {
        var submitted = 0;
        var errors = 0;

        for (var attempt = 0; ; attempt++)
        {
            var jobs = CreateJobs(migration, contents);
            if (jobs.Count == 0)
            {
                break;
            }

            // Only count the first attempt, because the retries are for the same contents.
            if (attempt == 0)
            {
                submitted = jobs.Count;
            }

            var command = new BulkUpdateContents
            {
                Actor = migration.Run.Actor,
                AppId = migration.App.NamedId(),
                Jobs = jobs.ToArray(),
                SchemaId = migration.Schema.NamedId(),
            };

            var commandContext = await commandBus.PublishAsync(command, ct);
            if (commandContext.PlainResult is not BulkUpdateResult result)
            {
                break;
            }

            var conflicts = new HashSet<DomainId>();

            // Errors are reported per content item, so that a single invalid item does not stop the migration.
            foreach (var item in result)
            {
                if (item.Exception == null)
                {
                    continue;
                }

                // The content has been changed after it has been read, therefore migrate it again with the new data.
                if (item.Exception is DomainObjectVersionException && item.Id != null && attempt < MaxRetries)
                {
                    conflicts.Add(item.Id.Value);
                    continue;
                }

                await migration.Run.LogAsync($"Failed to migrate content {item.Id}: {item.Exception.Message}");
                errors++;
            }

            if (conflicts.Count == 0)
            {
                break;
            }

            contents = await contentRepository.StreamWriteContents(migration.App.Id, [migration.Schema.Id], conflicts, ct).ToListAsync(ct);
        }

        return (submitted, errors);
    }

    private static List<BulkUpdateJob> CreateJobs(MigrationContext migration, List<WriteContent> contents)
    {
        var jobs = new List<BulkUpdateJob>(contents.Count);

        foreach (var content in contents)
        {
            // The current version is only the published version, if it has the published status. Otherwise it is a draft.
            var migrateCurrent =
                content.CurrentVersion.Status == Status.Published ?
                migration.MigratePublished :
                migration.MigrateDraft;

            var newCurrentData =
                migrateCurrent ?
                Convert(migration.Converter, content.CurrentVersion.Data) :
                null;

            var newDraftData =
                migration.MigrateDraft && content.NewVersion != null ?
                Convert(migration.Converter, content.NewVersion.Data) :
                null;

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

        return jobs;
    }

    private static ContentData? Convert(ContentConverter converter, ContentData data)
    {
        // The converter takes ownership of the data, therefore the clone is needed for the comparison.
        var converted = converter.Convert(data.Clone());

        return converted.Equals(data) ? null : converted;
    }
}
