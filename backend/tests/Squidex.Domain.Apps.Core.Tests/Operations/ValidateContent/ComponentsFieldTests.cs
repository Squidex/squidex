// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent;

public class ComponentsFieldTests : IClassFixture<TranslationsFixture>
{
    private readonly DomainId schemaId1 = DomainId.NewGuid();
    private readonly DomainId schemaId2 = DomainId.NewGuid();
    private readonly List<string> errors = new List<string>();

    [Fact]
    public void Should_instantiate_field()
    {
        var (_, sut, _) = Field(new ComponentsFieldProperties());

        Assert.Equal("myComponents", sut.Name);
    }

    [Fact]
    public async Task Should_not_add_error_if_components_are_null_and_valid()
    {
        var (_, sut, components) = Field(new ComponentsFieldProperties { SchemaId = schemaId1 });

        await sut.ValidateAsync(null, errors, components: components);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_components_is_valid()
    {
        var (id, sut, components) = Field(new ComponentsFieldProperties { SchemaId = schemaId1 });

        await sut.ValidateAsync(CreateValue(1, id.ToString(), "componentField", 1), errors, components: components);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_number_of_components_is_equal_to_min_and_max_components()
    {
        var (id, sut, components) = Field(new ComponentsFieldProperties { SchemaId = schemaId1, MinItems = 2, MaxItems = 2 });

        await sut.ValidateAsync(CreateValue(2, id.ToString(), "componentField", 1), errors, components: components);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_if_components_are_required()
    {
        var (_, sut, components) = Field(new ComponentsFieldProperties { SchemaId = schemaId1, IsRequired = true });

        await sut.ValidateAsync(null, errors, components: components);

        errors.Should().BeEquivalentTo(
            new[] { "Field is required." });
    }

    [Fact]
    public async Task Should_add_error_if_components_value_is_required()
    {
        var (id, sut, components) = Field(new ComponentsFieldProperties { SchemaId = schemaId1, IsRequired = true }, true);

        await sut.ValidateAsync(CreateValue(1, id.ToString(), "componentField", default), errors, components: components);

        errors.Should().BeEquivalentTo(
            new[] { "[1].componentField: Field is required." });
    }

    [Fact]
    public async Task Should_add_error_if_value_is_not_valid()
    {
        var (_, sut, components) = Field(new ComponentsFieldProperties { SchemaId = schemaId1 });

        await sut.ValidateAsync(JsonValue.Create("Invalid"), errors, components: components);

        errors.Should().BeEquivalentTo(
            new[] { "Invalid json type, expected array of objects." });
    }

    [Fact]
    public async Task Should_add_error_if_component_is_not_valid()
    {
        var (_, sut, components) = Field(new ComponentsFieldProperties { SchemaId = schemaId1 });

        await sut.ValidateAsync((JsonValue)JsonValue.Array(JsonValue.Create("Invalid")), errors, components: components);

        errors.Should().BeEquivalentTo(
            new[] { "Invalid json object, expected object with 'schemaId' field." });
    }

    [Fact]
    public async Task Should_add_error_if_component_has_no_discriminator()
    {
        var (_, sut, components) = Field(new ComponentsFieldProperties { SchemaIds = ReadonlyList.Create(schemaId1, schemaId2) });

        await sut.ValidateAsync(CreateValue(1, null, "field", 1), errors, components: components);

        errors.Should().BeEquivalentTo(
            new[] { "Invalid component. No 'schemaId' field found." });
    }

    [Fact]
    public async Task Should_add_error_if_value_has_invalid_discriminator_format()
    {
        var (_, sut, components) = Field(new ComponentsFieldProperties { SchemaId = schemaId1 });

        await sut.ValidateAsync(CreateValue(1, "invalid", "field", 1), errors, components: components);

        errors.Should().BeEquivalentTo(
            new[] { "Invalid component. Cannot find schema." });
    }

    [Fact]
    public async Task Should_add_error_if_value_has_invalid_discriminator_schema()
    {
        var (_, sut, components) = Field(new ComponentsFieldProperties { SchemaId = schemaId2 });

        await sut.ValidateAsync(CreateValue(1, schemaId1.ToString(), "field", 1), errors, components: components);

        errors.Should().BeEquivalentTo(
            new[] { "Invalid component. Cannot find schema." });
    }

    [Fact]
    public async Task Should_add_error_if_value_has_not_enough_components()
    {
        var (id, sut, components) = Field(new ComponentsFieldProperties { SchemaId = schemaId1, MinItems = 3 });

        await sut.ValidateAsync(CreateValue(2, id.ToString(), "componentField", 1), errors, components: components);

        errors.Should().BeEquivalentTo(
            new[] { "Must have at least 3 item(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_value_has_too_much_components()
    {
        var (id, sut, components) = Field(new ComponentsFieldProperties { SchemaId = schemaId1, MaxItems = 1 });

        await sut.ValidateAsync(CreateValue(2, id.ToString(), "componentField", 1), errors, components: components);

        errors.Should().BeEquivalentTo(
            new[] { "Must not have more than 1 item(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_value_has_duplicates()
    {
        var (id, sut, components) = Field(new ComponentsFieldProperties { UniqueFields = ReadonlyList.Create("componentField") });

        await sut.ValidateAsync(CreateValue(2, id.ToString(), "componentField", 1), errors, components: components);

        errors.Should().BeEquivalentTo(
            new[] { "Must not contain items with duplicate 'componentField' fields." });
    }

    [Fact]
    public async Task Should_resolve_schema_id_from_name()
    {
        var (_, sut, components) = Field(new ComponentsFieldProperties { SchemaId = schemaId1 });

        var value = CreateValue(1, "my-component", "componentField", 1, "schemaName");

        await sut.ValidateAsync(value, errors, components: components);

        Assert.Empty(errors);
        Assert.Equal(value.AsArray[0].AsObject[Component.Discriminator].AsString, schemaId1.ToString());
    }

    [Fact]
    public async Task Should_resolve_schema_from_single_component()
    {
        var (_, sut, components) = Field(new ComponentsFieldProperties { SchemaId = schemaId1 });

        var value = CreateValue(1, null, "componentField", 1);

        await sut.ValidateAsync(value, errors, components: components);

        Assert.Empty(errors);
        Assert.Equal(value.AsArray[0].AsObject[Component.Discriminator].AsString, schemaId1.ToString());
    }

    private static JsonValue CreateValue(int count, string? type, string key, JsonValue value, string? discriminator = null)
    {
        var actual = new JsonArray();

        for (var i = 0; i < count; i++)
        {
            var obj = new JsonObject();

            if (type != null)
            {
                discriminator ??= Component.Discriminator;

                obj[discriminator] = JsonValue.Create(type);
            }

            obj.Add(key, value);

            actual.Add(obj);
        }

        return actual;
    }

    private (DomainId, RootField<ComponentsFieldProperties>, ResolvedComponents) Field(ComponentsFieldProperties properties, bool isRequired = false)
    {
        var schema =
            new Schema("my-component")
                .AddNumber(1, "componentField", Partitioning.Invariant,
                    new NumberFieldProperties { IsRequired = isRequired });

        var field = Fields.Components(1, "myComponents", Partitioning.Invariant, properties);

        var components = new ResolvedComponents(new Dictionary<DomainId, Schema>
        {
            [schemaId1] = schema,
            [schemaId2] = schema
        });

        return (schemaId1, field, components);
    }
}
