// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class SchemaUpdateTests(CreatedAppFixture fixture) : IClassFixture<CreatedAppFixture>
{
    private readonly string schemaName = $"schema-{Guid.NewGuid()}";

    public CreatedAppFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_not_query_contents_of_unpublished_schema()
    {
        await CreateSchemaAsync();

        // STEP 1: Query the contents of the unpublished schema and fail.
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return _.Client.DynamicContents(schemaName).GetAsync();
        });

        Assert.Equal(404, ex.StatusCode);


        // STEP 2: Publish the schema and query again.
        await _.Client.Schemas.PublishSchemaAsync(schemaName);

        var contents = await _.Client.DynamicContents(schemaName).GetAsync();

        Assert.Empty(contents.Items);


        // STEP 3: Unpublish the schema again.
        var schema = await _.Client.Schemas.UnpublishSchemaAsync(schemaName);

        Assert.False(schema.IsPublished);
    }

    [Fact]
    public async Task Should_synchronize_schema()
    {
        await CreateSchemaAsync();

        await AddFieldAsync("field1");

        // STEP 1: Synchronize the schema with another field.
        var syncRequest = new SynchronizeSchemaDto
        {
            Category = "My Category",
            Fields =
            [
                new UpsertSchemaFieldDto
                {
                    Name = "field2",
                    Properties = new StringFieldPropertiesDto(),
                },
            ],
        };

        var schema_1 = await _.Client.Schemas.PutSchemaSyncAsync(schemaName, syncRequest);

        // The synchronization defines the target state, therefore the old field is deleted.
        Assert.Equal(["field2"], schema_1.Fields.Select(x => x.Name));
        Assert.Equal("My Category", schema_1.Category);


        // STEP 2: Synchronize the schema again, but keep the old fields.
        var syncRequest2 = new SynchronizeSchemaDto
        {
            NoFieldDeletion = true,
            Fields =
            [
                new UpsertSchemaFieldDto
                {
                    Name = "field3",
                    Properties = new StringFieldPropertiesDto(),
                },
            ],
        };

        var schema_2 = await _.Client.Schemas.PutSchemaSyncAsync(schemaName, syncRequest2);

        Assert.Equal(["field2", "field3"], schema_2.Fields.Select(x => x.Name).Order());
    }

    private async Task<SchemaDto> CreateSchemaAsync()
    {
        var createRequest = new CreateSchemaDto
        {
            Name = schemaName,
        };

        return await _.Client.Schemas.PostSchemaAsync(createRequest);
    }

    private async Task<SchemaDto> AddFieldAsync(string name)
    {
        var addRequest = new AddFieldDto
        {
            Name = name,
            Properties = new StringFieldPropertiesDto(),
        };

        return await _.Client.Schemas.PostFieldAsync(schemaName, addRequest);
    }
}
