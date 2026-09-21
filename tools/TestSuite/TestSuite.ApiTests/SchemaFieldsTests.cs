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

public class SchemaFieldsTests(CreatedAppFixture fixture) : IClassFixture<CreatedAppFixture>
{
    private readonly string schemaName = $"schema-{Guid.NewGuid()}";

    public CreatedAppFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_update_field()
    {
        await CreateSchemaAsync();

        var field = await AddFieldAsync("field1");

        // STEP 1: Update the properties of the field.
        var updateRequest = new UpdateFieldDto
        {
            Properties = new StringFieldPropertiesDto
            {
                Label = "My Label",
                IsRequired = true,
            },
        };

        var schema_1 = await _.Client.Schemas.PutFieldAsync(schemaName, FieldId(field, "field1"), updateRequest);

        var properties = Assert.IsType<StringFieldPropertiesDto>(schema_1.Fields.Single(x => x.Name == "field1").Properties);

        Assert.Equal("My Label", properties.Label);
        Assert.True(properties.IsRequired);
    }

    [Fact]
    public async Task Should_not_update_locked_field()
    {
        await CreateSchemaAsync();

        var field = await AddFieldAsync("field1");
        var fieldId = FieldId(field, "field1");

        // STEP 1: Lock the field.
        var schema_1 = await _.Client.Schemas.LockFieldAsync(schemaName, fieldId);

        Assert.True(schema_1.Fields.Single(x => x.Name == "field1").IsLocked);


        // STEP 2: Update the locked field and fail.
        var updateRequest = new UpdateFieldDto
        {
            Properties = new StringFieldPropertiesDto
            {
                Label = "My Label",
            },
        };

        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return _.Client.Schemas.PutFieldAsync(schemaName, fieldId, updateRequest);
        });

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task Should_add_and_delete_nested_field()
    {
        await CreateSchemaAsync();

        // STEP 1: Add an array field with a nested field.
        var parentId = await AddArrayFieldAsync();

        var schema_1 = await AddNestedFieldAsync(parentId, "nested1");

        var nested = NestedFields(schema_1).Single(x => x.Name == "nested1");

        Assert.False(nested.IsHidden);


        // STEP 2: Delete the nested field again.
        var schema_2 = await _.Client.Schemas.DeleteNestedFieldAsync(schemaName, parentId, nested.FieldId);

        Assert.DoesNotContain(NestedFields(schema_2), x => x.Name == "nested1");
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

    private async Task<long> AddArrayFieldAsync()
    {
        var addRequest = new AddFieldDto
        {
            Name = "array",
            Properties = new ArrayFieldPropertiesDto(),
        };

        var schema = await _.Client.Schemas.PostFieldAsync(schemaName, addRequest);

        return FieldId(schema, "array");
    }

    private async Task<SchemaDto> AddNestedFieldAsync(long parentId, string name)
    {
        var addRequest = new AddFieldDto
        {
            Name = name,
            Properties = new StringFieldPropertiesDto(),
        };

        return await _.Client.Schemas.PostNestedFieldAsync(schemaName, parentId, addRequest);
    }

    private static long FieldId(SchemaDto schema, string name)
    {
        return schema.Fields.Single(x => x.Name == name).FieldId;
    }

    private static List<NestedFieldDto> NestedFields(SchemaDto schema)
    {
        return schema.Fields.Single(x => x.Name == "array").Nested ?? [];
    }
}
