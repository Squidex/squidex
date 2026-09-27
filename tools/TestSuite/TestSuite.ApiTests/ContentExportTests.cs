// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentAssertions;
using Newtonsoft.Json;
using Squidex.ClientLibrary;
using TestSuite.Fixtures;
using TestSuite.Model;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class ContentExportTests(ClientFixture fixture) : IClassFixture<ClientFixture>
{
    private readonly string appName = Guid.NewGuid().ToString();
    private readonly string schemaName = $"schema-{Guid.NewGuid()}";

    public ClientFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_export_contents_as_csv()
    {
        // STEP 0: Create app, because jobs cannot run in parallel within the same app.
        var (app, appDto) = await _.PostAppAsync(appName);


        // STEP 1: Create contents, including a text with a line break that must be quoted.
        await TestEntity.CreateSchemaAsync(app.Schemas, schemaName);

        var contents = app.Contents<TestEntity, TestEntityData>(schemaName);

        await contents.CreateAsync(new TestEntityData { Number = 1, String = "Hello" }, ContentCreateOptions.AsPublish);
        await contents.CreateAsync(new TestEntityData { Number = 2, String = "Hello\nWorld" }, ContentCreateOptions.AsPublish);


        // STEP 2: Export the contents.
        var file = await ExportAsync(app, appDto, new ExportContentsDto
        {
            Format = ExportFormat.Csv,
            Fields = "Number=data.number,String=data.string",
        });


        // STEP 3: Check the file, the order of the contents is not defined.
        var lines = file.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("Number,String", lines[0]);
        Assert.Equal(["1,Hello", "2,\"Hello\nWorld\""], lines.Skip(1).Order());
    }

    [Fact]
    public async Task Should_export_contents_as_json()
    {
        // STEP 0: Create app, because jobs cannot run in parallel within the same app.
        var (app, appDto) = await _.PostAppAsync(appName);


        // STEP 1: Create contents.
        await TestEntity.CreateSchemaAsync(app.Schemas, schemaName);

        var contents = app.Contents<TestEntity, TestEntityData>(schemaName);

        var content_1 = await contents.CreateAsync(new TestEntityData { Number = 1, String = "Hello" }, ContentCreateOptions.AsPublish);
        var content_2 = await contents.CreateAsync(new TestEntityData { Number = 2 }, ContentCreateOptions.AsPublish);


        // STEP 2: Export the contents.
        var file = await ExportAsync(app, appDto, new ExportContentsDto
        {
            Format = ExportFormat.Json,
            Fields = "Id=id,Number=data.number,String=data.string",
        });


        // STEP 3: Check the file, missing values are exported as null.
        var items = JsonConvert.DeserializeObject<List<ExportedItem>>(file);

        items.Should().BeEquivalentTo(
            [
                new ExportedItem(content_1.Id, 1, "Hello"),
                new ExportedItem(content_2.Id, 2, null),
            ]);
    }

    [Fact]
    public async Task Should_export_published_contents_only()
    {
        // STEP 0: Create app, because jobs cannot run in parallel within the same app.
        var (app, appDto) = await _.PostAppAsync(appName);


        // STEP 1: Create a published and an unpublished content.
        await TestEntity.CreateSchemaAsync(app.Schemas, schemaName);

        var contents = app.Contents<TestEntity, TestEntityData>(schemaName);

        await contents.CreateAsync(new TestEntityData { Number = 1 }, ContentCreateOptions.AsPublish);
        await contents.CreateAsync(new TestEntityData { Number = 2 });


        // STEP 2: Export the contents.
        var file = await ExportAsync(app, appDto, new ExportContentsDto
        {
            Fields = "Number=data.number",
        });


        // STEP 3: Only the published content has been exported.
        var lines = file.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(["Number", "1"], lines);
    }

    [Fact]
    public async Task Should_export_unpublished_contents_if_requested()
    {
        // STEP 0: Create app, because jobs cannot run in parallel within the same app.
        var (app, appDto) = await _.PostAppAsync(appName);


        // STEP 1: Create a published and an unpublished content.
        await TestEntity.CreateSchemaAsync(app.Schemas, schemaName);

        var contents = app.Contents<TestEntity, TestEntityData>(schemaName);

        await contents.CreateAsync(new TestEntityData { Number = 1 }, ContentCreateOptions.AsPublish);
        await contents.CreateAsync(new TestEntityData { Number = 2 });


        // STEP 2: Export the contents.
        var file = await ExportAsync(app, appDto, new ExportContentsDto
        {
            Fields = "Number=data.number",
            Unpublished = true,
        });


        // STEP 3: Both contents have been exported.
        var lines = file.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("Number", lines[0]);
        Assert.Equal(["1", "2"], lines.Skip(1).Order());
    }

    [Fact]
    public async Task Should_not_start_export_with_invalid_fields()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync(appName);


        // STEP 1: Create a schema.
        await TestEntity.CreateSchemaAsync(app.Schemas, schemaName);


        // STEP 2: Start the export.
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return app.Schemas.PostContentExportAsync(schemaName, new ExportContentsDto
            {
                Fields = "unknown",
            });
        });

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task Should_not_start_export_for_unknown_schema()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync(appName);


        // STEP 1: Start the export.
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return app.Schemas.PostContentExportAsync(schemaName, new ExportContentsDto());
        });

        Assert.Equal(404, ex.StatusCode);
    }

    private async Task<string> ExportAsync(ISquidexClient app, AppDto appDto, ExportContentsDto request)
    {
        // Use a reference to find the job of this export.
        var reference = Guid.NewGuid().ToString();

        await app.Schemas.PostContentExportAsync(schemaName, request, reference);

        var job = await app.Jobs.PollAsync(x => x.Reference == reference && x.Status is JobStatus.Completed or JobStatus.Failed);

        Assert.Equal(JobStatus.Completed, job?.Status);

        var download = await app.Jobs.GetJobContentAsync(job!.Id, appDto.Id);

        using var reader = new StreamReader(download.Stream);

        return await reader.ReadToEndAsync();
    }

    private sealed record ExportedItem(string Id, int Number, string? String);
}
