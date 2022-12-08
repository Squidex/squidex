// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

[UsesVerify]
public class SchemaTests : IClassFixture<CreatedAppFixture>
{
    private readonly string schemaName = $"schema-{Guid.NewGuid()}";

    public CreatedAppFixture _ { get; }

    public SchemaTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_schema()
    {
        // STEP 1: Create schema
        var createRequest = new CreateSchemaDto
        {
            Name = schemaName
        };

        var schema = await _.Schemas.PostSchemaAsync(_.AppName, createRequest);

        // Should return created schemas with correct name.
        Assert.Equal(schemaName, schema.Name);


        // STEP 2: Get all schemas
        var schemas = await _.Schemas.GetSchemasAsync(_.AppName);

        // Should provide new schema when apps are schemas.
        Assert.Contains(schemas.Items, x => x.Name == schemaName);
    }

    [Fact]
    public async Task Should_not_allow_creation_if_name_used()
    {
        // STEP 1: Create schema
        var createRequest = new CreateSchemaDto
        {
            Name = schemaName
        };

        var schema = await _.Schemas.PostSchemaAsync(_.AppName, createRequest);


        // STEP 2: Create again and fail
        var ex = await Assert.ThrowsAnyAsync<SquidexManagementException>(() =>
        {
            return _.Schemas.PostSchemaAsync(_.AppName, createRequest);
        });

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task Should_create_singleton_schema()
    {
        // STEP 1: Create schema
        var createRequest = new CreateSchemaDto
        {
            Name = schemaName,
            // Use the new property to create a singleton.
            Type = SchemaType.Singleton,
            // Must be pusblished to query content.
            IsPublished = true
        };

        var schema = await _.Schemas.PostSchemaAsync(_.AppName, createRequest);

        // Should return created schemas with correct name.
        Assert.Equal(schemaName, schema.Name);

        await Verify(schema)
            .IgnoreMember<SchemaDto>(x => x.Name);


        // STEP 2: Get all schemas
        var schemas = await _.Schemas.GetSchemasAsync(_.AppName);

        // Should provide new schema when apps are schemas.
        Assert.Contains(schemas.Items, x => x.Name == schemaName);


        // STEP 3: Get singleton content
        var client = _.ClientManager.CreateDynamicContentsClient(schemaName);

        var content = await client.GetAsync(schema.Id);

        Assert.NotNull(content);
    }

    [Fact]
    public async Task Should_create_singleton_schema_with_obsolete_property()
    {
        // STEP 1: Create schema
        var createRequest = new CreateSchemaDto
        {
            Name = schemaName,
            // Use the old property to create a singleton.
            IsSingleton = true,
            // Must be pusblished to query content.
            IsPublished = true
        };

        var schema = await _.Schemas.PostSchemaAsync(_.AppName, createRequest);

        // Should return created schemas with correct name.
        Assert.Equal(schemaName, schema.Name);

        await Verify(schema)
            .IgnoreMember<SchemaDto>(x => x.Name);


        // STEP 2: Get all schemas
        var schemas = await _.Schemas.GetSchemasAsync(_.AppName);

        // Should provide new schema when apps are schemas.
        Assert.Contains(schemas.Items, x => x.Name == schemaName);


        // STEP 3: Get singleton content
        var client = _.ClientManager.CreateDynamicContentsClient(schemaName);

        var content = await client.GetAsync(schema.Id);

        Assert.NotNull(content);
    }

    [Fact]
    public async Task Should_create_schema_with_checkboxes()
    {
        // STEP 1: Create schema
        var createRequest = new CreateSchemaDto
        {
            Name = schemaName,
            Fields = new List<UpsertSchemaFieldDto>
            {
                new UpsertSchemaFieldDto
                {
                    Name = "references",
                    Partitioning = "invariant",
                    Properties = new ReferencesFieldPropertiesDto
                    {
                        Editor = ReferencesFieldEditor.Checkboxes
                    }
                },
                new UpsertSchemaFieldDto
                {
                    Name = "tags",
                    Partitioning = "invariant",
                    Properties = new TagsFieldPropertiesDto
                    {
                        Editor = TagsFieldEditor.Checkboxes,
                        AllowedValues = new List<string> { "value1" }
                    }
                }
            }
        };

        var schema = await _.Schemas.PostSchemaAsync(_.AppName, createRequest);

        // Should return created schemas with correct name.
        Assert.Equal(schemaName, schema.Name);
    }

    [Fact]
    public async Task Should_delete_Schema()
    {
        // STEP 1: Create schema
        var createRequest = new CreateSchemaDto
        {
            Name = schemaName
        };

        var schema = await _.Schemas.PostSchemaAsync(_.AppName, createRequest);

        // Should return created schemas with correct name.
        Assert.Equal(schemaName, schema.Name);


        // STEP 2: Delete schema
        await _.Schemas.DeleteSchemaAsync(_.AppName, schemaName);

        var schemas = await _.Schemas.GetSchemasAsync(_.AppName);

        // Should not provide deleted schema when schema are queried.
        Assert.DoesNotContain(schemas.Items, x => x.Name == schemaName);
    }

    [Fact]
    public async Task Should_recreate_after_deleted()
    {
        // STEP 1: Create schema
        var createRequest = new CreateSchemaDto
        {
            Name = schemaName
        };

        var schema = await _.Schemas.PostSchemaAsync(_.AppName, createRequest);

        // Should return created schemas with correct name.
        Assert.Equal(schemaName, schema.Name);


        // STEP 2: Delete schema.
        await _.Schemas.DeleteSchemaAsync(_.AppName, schemaName);


        // STEP 3: Create app again
        await _.Schemas.PostSchemaAsync(_.AppName, createRequest);
    }
}
