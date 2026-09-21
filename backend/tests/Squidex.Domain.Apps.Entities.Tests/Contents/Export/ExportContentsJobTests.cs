// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Text;
using Squidex.Assets;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Validation;
using CsvReader = CsvHelper.CsvReader;
using IClock = NodaTime.IClock;
using TestUtils = Squidex.Domain.Apps.Core.TestHelpers.TestUtils;

namespace Squidex.Domain.Apps.Entities.Contents.Export;

public class ExportContentsJobTests : GivenContext
{
    private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
    private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
    private readonly ExportContentsJob sut;
    private string? uploadedFile;

    public ExportContentsJobTests()
    {
        Schema =
            Schema
                .AddString(1, "my-field", Partitioning.Invariant);

        A.CallTo(() => assetStore.UploadAsync(A<string>._, A<Stream>._, true, A<CancellationToken>._))
            .Invokes(x =>
            {
                using var reader = new StreamReader(x.GetArgument<Stream>(1)!, Encoding.UTF8, leaveOpen: true);

                uploadedFile = reader.ReadToEnd();
            });

        sut = new ExportContentsJob(AppProvider, assetStore, contentQuery, TestUtils.DefaultSerializer);
    }

    [Fact]
    public void Should_create_request()
    {
        var job = ExportContentsJob.BuildRequest(User, App, Schema, ExportFormat.Json, "id", true);

        job.Arguments.Should().BeEquivalentTo(
            new Dictionary<string, string>
            {
                ["appId"] = App.Id.ToString(),
                ["appName"] = App.Name,
                ["schemaId"] = Schema.Id.ToString(),
                ["schemaName"] = Schema.Name,
                ["format"] = "Json",
                ["fields"] = "id",
                ["unpublished"] = "True",
            });
    }

    [Fact]
    public void Should_throw_exception_if_fields_are_not_valid()
    {
        Assert.Throws<ValidationException>(() => ExportContentsJob.BuildRequest(User, App, Schema, ExportFormat.Csv, fields: "invalid"));
    }

    [Fact]
    public async Task Should_throw_exception_if_schema_not_found()
    {
        A.CallTo(() => AppProvider.GetAppWithSchemaAsync(AppId.Id, SchemaId.Id, A<bool>._, A<CancellationToken>._))
            .Returns((App, null as Schema));

        var context = CreateRunContext(CreateJob(ExportFormat.Csv));

        await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.RunAsync(context, CancellationToken));
    }

    [Fact]
    public async Task Should_export_csv_with_default_fields()
    {
        var content = CreateContent("Hello\nWorld");

        SetupStream(content);

        var context = CreateRunContext(CreateJob(ExportFormat.Csv));

        await sut.RunAsync(context, CancellationToken);

        // Read the file back, so that we also test that line breaks are quoted correctly.
        var rows = ReadCsv(uploadedFile!);

        rows.Should().BeEquivalentTo(
            new[]
            {
                new[] { "id", "created", "createdBy", "lastModified", "lastModifiedBy", "status", "version", "data.my-field.iv" },
                new[]
                {
                    content.Id.ToString(),
                    content.Created.ToString("g", CultureInfo.InvariantCulture),
                    content.CreatedBy.ToString(),
                    content.LastModified.ToString("g", CultureInfo.InvariantCulture),
                    content.LastModifiedBy.ToString(),
                    "Published",
                    "1",
                    "Hello\nWorld",
                },
            },
            options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Should_export_json_with_custom_fields()
    {
        SetupStream(CreateContent("Hello"), CreateContent("World"));

        var context = CreateRunContext(CreateJob(ExportFormat.Json, fields: "Text=data.my-field"));

        await sut.RunAsync(context, CancellationToken);

        Assert.Equal("[\n{\"Text\":\"Hello\"},\n{\"Text\":\"World\"}\n]", uploadedFile);
    }

    [Fact]
    public async Task Should_export_empty_json_array_if_no_content_found()
    {
        SetupStream();

        var context = CreateRunContext(CreateJob(ExportFormat.Json));

        await sut.RunAsync(context, CancellationToken);

        Assert.Equal("[]", uploadedFile);
    }

    [Fact]
    public async Task Should_set_file_and_description()
    {
        SetupStream();

        var context = CreateRunContext(CreateJob(ExportFormat.Json));

        await sut.RunAsync(context, CancellationToken);

        Assert.Equal("application/json", context.Job.File?.MimeType);
        Assert.EndsWith(".json", context.Job.File?.Name, StringComparison.Ordinal);
        Assert.Equal($"Schema {Schema.Name}: Export contents", context.Job.Description);
    }

    [Fact]
    public async Task Should_log_progress_as_single_line()
    {
        SetupStream(Enumerable.Range(0, 250).Select(x => CreateContent($"{x}")).ToArray());

        var context = CreateRunContext(CreateJob(ExportFormat.Csv, fields: "data.my-field"));

        await sut.RunAsync(context, CancellationToken);

        var lines = uploadedFile!.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(251, lines.Length);
        Assert.Equal("Exported contents: 250", Assert.Single(context.Job.Log).Message);
    }

    [Fact]
    public async Task Should_download_file_from_asset_store()
    {
        var job = CreateJob(ExportFormat.Csv);

        var stream = new MemoryStream();

        await sut.DownloadAsync(job, stream, CancellationToken);

        A.CallTo(() => assetStore.DownloadAsync($"export-{job.Id}", stream, default, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_delete_file_from_asset_store_on_cleanup()
    {
        var job = CreateJob(ExportFormat.Csv);

        await sut.CleanupAsync(job);

        A.CallTo(() => assetStore.DeleteAsync($"export-{job.Id}", default))
            .MustHaveHappened();
    }

    private EnrichedContent CreateContent(string value)
    {
        return CreateContent() with
        {
            Data =
                new ContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddInvariant(value)),
        };
    }

    private static List<string[]> ReadCsv(string csv)
    {
        using var reader = new CsvReader(new StringReader(csv), CultureInfo.InvariantCulture);

        var rows = new List<string[]>();

        while (reader.Read())
        {
            rows.Add(reader.Parser.Record!);
        }

        return rows;
    }

    private void SetupStream(params EnrichedContent[] contents)
    {
        A.CallTo(() => contentQuery.StreamAsync(A<Context>._, Schema.Id.ToString(), 0, CancellationToken))
            .Returns(contents.ToAsyncEnumerable());
    }

    private Job CreateJob(ExportFormat format, string? fields = null)
    {
        return new Job
        {
            Id = DomainId.NewGuid(),
            Arguments = ExportContentsJob.BuildRequest(User, App, Schema, format, fields).Arguments,
        };
    }

    private JobRunContext CreateRunContext(Job job)
    {
        var state = new SimpleState<JobsState>(A.Fake<IPersistenceFactory<JobsState>>(), GetType(), App.Id);

        return new JobRunContext(state, A.Fake<IClock>(), default) { Actor = User, Job = job, OwnerId = App.Id };
    }
}
