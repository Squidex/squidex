// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Fixtures;
using TestSuite.Model;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class ContentMigrationTests(ClientFixture fixture) : IClassFixture<ClientFixture>
{
    private readonly string appName = Guid.NewGuid().ToString();
    private readonly string schemaName = $"schema-{Guid.NewGuid()}";

    public ClientFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_migrate_contents_with_invalid_values()
    {
        // STEP 0: Create app, because jobs cannot run in parallel within the same app.
        var (app, _) = await _.PostAppAsync(appName);


        // STEP 1: Create a schema.
        var schema = await TestEntity.CreateSchemaAsync(app.Schemas, schemaName);

        var contents = app.Contents<TestEntity, TestEntityData>(schemaName);


        // STEP 2: Create a content that becomes invalid and a content that stays valid.
        var contentInvalid_1 = await contents.CreateAsync(
            new TestEntityData
            {
                String = "hello",
            },
            ContentCreateOptions.AsPublish);

        var contentValid_1 = await contents.CreateAsync(
            new TestEntityData
            {
                Number = 42,
            },
            ContentCreateOptions.AsPublish);


        // STEP 3: Change the type of the field, so that the stored value is not valid anymore.
        await ChangeStringFieldToNumberAsync(app, schema);


        // STEP 4: Start the migration with the default options.
        await app.Schemas.PostContentMigrationAsync(schemaName, new MigrateContentsDto());


        // STEP 5: Wait for the migration to complete.
        var job = await WaitForMigrationAsync(app);

        Assert.Equal("Checked contents: 2, submitted: 1", Assert.Single(job.Log).Message);


        // STEP 6: Only the invalid content has been updated.
        var contentInvalid_2 = await contents.GetAsync(contentInvalid_1.Id);
        var contentValid_2 = await contents.GetAsync(contentValid_1.Id);

        Assert.Equal(contentInvalid_1.Version + 1, contentInvalid_2.Version);
        Assert.Equal(contentValid_1.Version, contentValid_2.Version);
    }

    [Fact]
    public async Task Should_migrate_published_version_of_content_with_draft()
    {
        // STEP 0: Create app, because jobs cannot run in parallel within the same app.
        var (app, _) = await _.PostAppAsync(appName);


        // STEP 1: Create a published content with an invalid value and a valid draft.
        var (schema, contents, content_1) = await CreateContentWithDraftAsync(app);


        // STEP 2: Change the type of the field, so that the stored value is not valid anymore.
        await ChangeStringFieldToNumberAsync(app, schema);


        // STEP 3: Only migrate the published version, because the draft is valid anyway.
        await app.Schemas.PostContentMigrationAsync(schemaName, new MigrateContentsDto
        {
            MigrateDraft = false,
        });

        await WaitForMigrationAsync(app);


        // STEP 4: The published version has been migrated.
        var content_2 = await contents.GetAsync(content_1.Id, QueryContext.Default.Unpublished(true));

        Assert.Equal(content_1.Version + 1, content_2.Version);
    }

    [Fact]
    public async Task Should_not_migrate_published_version_if_excluded()
    {
        // STEP 0: Create app, because jobs cannot run in parallel within the same app.
        var (app, _) = await _.PostAppAsync(appName);


        // STEP 1: Create a published content with an invalid value and a valid draft.
        var (schema, contents, content_1) = await CreateContentWithDraftAsync(app);


        // STEP 2: Change the type of the field, so that the stored value is not valid anymore.
        await ChangeStringFieldToNumberAsync(app, schema);


        // STEP 3: Only migrate the draft version, which is valid.
        await app.Schemas.PostContentMigrationAsync(schemaName, new MigrateContentsDto
        {
            MigratePublished = false,
        });

        await WaitForMigrationAsync(app);


        // STEP 4: Nothing has been changed.
        var content_2 = await contents.GetAsync(content_1.Id, QueryContext.Default.Unpublished(true));

        Assert.Equal(content_1.Version, content_2.Version);
    }

    [Fact]
    public async Task Should_not_start_migration_for_unknown_schema()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync(appName);


        // STEP 1: Start the migration.
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return app.Schemas.PostContentMigrationAsync(schemaName, new MigrateContentsDto());
        });

        Assert.Equal(404, ex.StatusCode);
    }

    private async Task<(SchemaDto, IContentsClient<TestEntity, TestEntityData>, TestEntity)> CreateContentWithDraftAsync(ISquidexClient app)
    {
        var schema = await TestEntity.CreateSchemaAsync(app.Schemas, schemaName);

        var contents = app.Contents<TestEntity, TestEntityData>(schemaName);

        var content = await contents.CreateAsync(
            new TestEntityData
            {
                String = "hello",
            },
            ContentCreateOptions.AsPublish);

        await contents.CreateDraftAsync(content.Id);

        // The draft does not have a value for the field, therefore it stays valid after the schema change.
        content = await contents.UpdateAsync(content.Id,
            new TestEntityData
            {
                Number = 42,
            });

        return (schema, contents, content);
    }

    private async Task ChangeStringFieldToNumberAsync(ISquidexClient app, SchemaDto schema)
    {
        await app.Schemas.DeleteFieldAsync(schemaName, schema.Fields.First(x => x.Name == TestEntityData.StringField).FieldId);

        await app.Schemas.PostFieldAsync(schemaName, new AddFieldDto
        {
            Name = TestEntityData.StringField,
            Properties = new NumberFieldPropertiesDto
            {
                IsRequired = false,
            },
        });
    }

    private static async Task<JobDto> WaitForMigrationAsync(ISquidexClient app)
    {
        var job = await app.Jobs.PollAsync(x => x.TaskName == "migrateContents" && x.Status is JobStatus.Completed or JobStatus.Failed);

        Assert.Equal(JobStatus.Completed, job?.Status);

        return job!;
    }
}
