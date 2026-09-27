// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Contents.Export;

public sealed class ExportContentsJob(
    IAppProvider appProvider,
    IAssetStore assetStore,
    IContentQueryService contentQuery,
    IJsonSerializer jsonSerializer)
    : IJobRunner
{
    public const string TaskName = "exportContents";
    public const string ArgAppId = "appId";
    public const string ArgAppName = "appName";
    public const string ArgSchemaId = "schemaId";
    public const string ArgSchemaName = "schemaName";
    public const string ArgFormat = "format";
    public const string ArgFields = "fields";
    public const string ArgUnpublished = "unpublished";

    private const int ProgressInterval = 100;

    public string Name => TaskName;

    public int MaxJobs => 10;

    public static JobRequest BuildRequest(RefToken actor, App app, Schema schema, ExportFormat format,
        string? fields = null,
        bool unpublished = false)
    {
        Guard.NotNull(actor);
        Guard.NotNull(app);
        Guard.NotNull(schema);

        var arguments = new Dictionary<string, string>
        {
            [ArgAppId] = app.Id.ToString(),
            [ArgAppName] = app.Name,
            [ArgSchemaId] = schema.Id.ToString(),
            [ArgSchemaName] = schema.Name,
            [ArgFormat] = format.ToString(),
            [ArgUnpublished] = unpublished.ToString(),
        };

        if (!string.IsNullOrWhiteSpace(fields))
        {
            // Validate the fields early, so that the caller gets an error instead of a failed job.
            ContentExportMapping.Parse(fields);

            arguments[ArgFields] = fields;
        }

        return JobRequest.Create(actor, TaskName, arguments) with
        {
            AppId = app.NamedId(),
        };
    }

    public Task DownloadAsync(Job job, Stream stream,
        CancellationToken ct)
    {
        return assetStore.DownloadAsync(GetFileName(job.Id), stream, default, ct);
    }

    public Task CleanupAsync(Job job)
    {
        return assetStore.DeleteAsync(GetFileName(job.Id), default);
    }

    public async Task RunAsync(JobRunContext context,
        CancellationToken ct)
    {
        var schemaId = context.GetArgumentId(ArgSchemaId);
        var schemaName = context.GetArgument(ArgSchemaName);

        if (!Enum.TryParse<ExportFormat>(context.TryGetArgument(ArgFormat), true, out var format))
        {
            format = ExportFormat.Csv;
        }

        var (app, schema) = await appProvider.GetAppWithSchemaAsync(context.OwnerId, schemaId, ct: ct);
        if (app == null)
        {
            throw new DomainObjectNotFoundException(context.OwnerId.ToString());
        }

        if (schema == null)
        {
            throw new DomainObjectNotFoundException(schemaId.ToString());
        }

        var fields = context.TryGetArgument(ArgFields);

        var mapping =
            !string.IsNullOrWhiteSpace(fields) ?
            ContentExportMapping.Parse(fields) :
            ContentExportMapping.CreateDefault(app, schema, format);

        // Use a readable name to describe the job.
        context.Job.Description = $"Schema {schemaName}: Export contents";

        // The file is stored in the asset store and made available for download through the job.
        context.Job.File = format == ExportFormat.Json ?
            new JobFile($"contents-{schemaName}-{context.Job.Started:yyyy-MM-dd_HH-mm-ss}.json", "application/json") :
            new JobFile($"contents-{schemaName}-{context.Job.Started:yyyy-MM-dd_HH-mm-ss}.csv", "text/csv");

        // The job is not running in the scope of a request, but the API has checked the read permissions.
        var queryContext = Context.Admin(app).Clone(b => b
            .WithNoCacheKeys()
            .WithNoEnrichment()
            .WithNoScripting()
            .WithNoTotal()
            .WithUnpublished(context.GetArgumentFlag(ArgUnpublished)));

        await using var stream = OpenTempStream(context.Job.Id);

        await using (var writer = CreateWriter(format, stream, mapping))
        {
            await writer.StartAsync(ct);

            var totalExported = 0;
            // Stream the contents without filtering or sorting, to keep the load on the database low.
            await foreach (var content in contentQuery.StreamAsync(queryContext, schema.Id.ToString(), 0, ct))
            {
                await writer.WriteAsync(content, ct);

                totalExported++;
                if (totalExported % ProgressInterval == 0)
                {
                    await context.LogAsync($"Exported contents: {totalExported}", totalExported > ProgressInterval);
                }
            }

            await writer.CompleteAsync(ct);

            await context.LogAsync($"Exported contents: {totalExported}", totalExported >= ProgressInterval);
        }

        stream.Position = 0;
        ct.ThrowIfCancellationRequested();

        await assetStore.UploadAsync(GetFileName(context.Job.Id), stream, true, ct);
    }

    private IContentExportWriter CreateWriter(ExportFormat format, Stream stream, ContentExportMapping mapping)
    {
        if (format == ExportFormat.Json)
        {
            return new JsonContentExportWriter(stream, mapping, jsonSerializer);
        }

        return new CsvContentExportWriter(stream, mapping, jsonSerializer);
    }

    private static FileStream OpenTempStream(DomainId jobId)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"export-{jobId}");

        return new FileStream(
            tempFile,
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.None,
            4096,
            FileOptions.DeleteOnClose);
    }

    private static string GetFileName(DomainId jobId)
    {
        return $"export-{jobId}";
    }
}
