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

public class ComponentFieldTests : IClassFixture<TranslationsFixture>
{
    private readonly DomainId schemaId1 = DomainId.NewGuid();
    private readonly DomainId schemaId2 = DomainId.NewGuid();
    private readonly List<string> errors = new List<string>();

    [Fact]
    public void Should_instantiate_field()
    {
        var (_, sut, _) = Field(new ComponentFieldProperties());

        Assert.Equal("myComponent", sut.Name);
    }

    [Fact]
    public async Task Should_not_add_error_if_component_is_null_and_valid()
    {
        var (_, sut, components) = Field(new ComponentFieldProperties { SchemaId = schemaId1 });

        await sut.ValidateAsync(null, errors, components: components);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_component_is_valid()
    {
        var (id, sut, components) = Field(new ComponentFieldProperties { SchemaId = schemaId1 });

        await sut.ValidateAsync(CreateValue(id.ToString(), "componentField", 1), errors, components: components);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_if_component_is_required()
    {
        var (_, sut, components) = Field(new ComponentFieldProperties { SchemaId = schemaId1, IsRequired = true });

        await sut.ValidateAsync(null, errors, components: components);

        errors.Should().BeEquivalentTo(
            new[] { "Field is required." });
    }

    [Fact]
    public async Task Should_add_error_if_component_value_is_required()
    {
        var (id, sut, components) = Field(new ComponentFieldProperties { SchemaId = schemaId1, IsRequired = true }, true);

        await sut.ValidateAsync(CreateValue(id.ToString(), "componentField", default), errors, components: components);

        errors.Should().BeEquivalentTo(
            new[] { "componentField: Field is required." });
    }

    [Fact]
    public async Task Should_add_error_if_value_is_not_valid()
    {
        var (_, sut, components) = Field(new ComponentFieldProperties { SchemaId = schemaId1 });

        await sut.ValidateAsync(JsonValue.Create("Invalid"), errors, components: components);

        errors.Should().BeEquivalentTo(
            new[] { "Invalid json object, expected object with 'schemaId' field." });
    }

    [Fact]
    public async Task Should_add_error_if_value_has_no_discriminator()
    {
        var (_, sut, components) = Field(new ComponentFieldProperties { SchemaIds = ReadonlyList.Create(schemaId1, schemaId2) });

        await sut.ValidateAsync(CreateValue(null, "field", 1), errors, components: components);

        errors.Should().BeEquivalentTo(
            new[] { "Invalid component. No 'schemaId' field found." });
    }

    [Fact]
    public async Task Should_add_error_if_value_has_invalid_discriminator_format()
    {
        var (_, sut, components) = Field(new ComponentFieldProperties { SchemaId = schemaId1 });

        await sut.ValidateAsync(CreateValue("invalid", "field", 1), errors, components: components);

        errors.Should().BeEquivalentTo(
            new[] { "Invalid component. Cannot find schema." });
    }

    [Fact]
    public async Task Should_add_error_if_value_has_invalid_discriminator_schema()
    {
        var (_, sut, components) = Field(new ComponentFieldProperties { SchemaId = schemaId2 });

        await sut.ValidateAsync(CreateValue(schemaId1.ToString(), "field", 1), errors, components: components);

        errors.Should().BeEquivalentTo(
            new[] { "Invalid component. Cannot find schema." });
    }

    [Fact]
    public async Task Should_resolve_schema_id_from_name()
    {
        var (_, sut, components) = Field(new ComponentFieldProperties { SchemaId = schemaId1 });

        var value = CreateValue("my-component", "componentField", 1, "schemaName");

        await sut.ValidateAsync(value, errors, components: components);

        Assert.Empty(errors);
        Assert.Equal(value.AsObject[Component.Discriminator].AsString, schemaId1.ToString());
    }

    [Fact]
    public async Task Should_resolve_schema_from_single_component()
    {
        var (_, sut, components) = Field(new ComponentFieldProperties { SchemaId = schemaId1 });

        var value = CreateValue(null, "componentField", 1);

        await sut.ValidateAsync(value, errors, components: components);

        Assert.Empty(errors);
        Assert.Equal(value.AsObject[Component.Discriminator].AsString, schemaId1.ToString());
    }

    private static JsonValue CreateValue(string? type, string key, JsonValue value, string? discriminator = null)
    {
        var obj = new JsonObject();

        if (type != null)
        {
            discriminator ??= Component.Discriminator;

            obj[discriminator] = JsonValue.Create(type);
        }

        obj.Add(key, value);

        return obj;
    }

    private (DomainId, RootField<ComponentFieldProperties>, ResolvedComponents) Field(ComponentFieldProperties properties, bool isRequired = false)
    {
        var schema =
            new Schema("my-component")
                .AddNumber(1, "componentField", Partitioning.Invariant,
                    new NumberFieldProperties { IsRequired = isRequired });

        var field = Fields.Component(1, "myComponent", Partitioning.Invariant, properties);

        var components = new ResolvedComponents(new Dictionary<DomainId, Schema>
        {
            [schemaId1] = schema,
            [schemaId2] = schema
        });

        return (schemaId1, field, components);
    }
}
