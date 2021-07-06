// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class ComponentsFieldTests : IClassFixture<TranslationsFixture>
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var (_, sut, _) = Field(new ComponentsFieldProperties());

            Assert.Equal("my-components", sut.Name);
        }

        [Fact]
        public async Task Should_not_add_error_if_components_are_null_and_valid()
        {
            var (_, sut, components) = Field(new ComponentsFieldProperties());

            await sut.ValidateAsync(null, errors, components: components);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_components_is_valid()
        {
            var (id, sut, components) = Field(new ComponentsFieldProperties());

            await sut.ValidateAsync(CreateValue(1, id.ToString(), "component-field", 1), errors, components: components);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_number_of_components_is_equal_to_min_and_max_components()
        {
            var (id, sut, components) = Field(new ComponentsFieldProperties { MinItems = 2, MaxItems = 2 });

            await sut.ValidateAsync(CreateValue(2, id.ToString(), "component-field", 1), errors, components: components);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_components_are_required()
        {
            var (_, sut, components) = Field(new ComponentsFieldProperties { IsRequired = true });

            await sut.ValidateAsync(null, errors, components: components);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_error_if_components_value_is_required()
        {
            var (id, sut, components) = Field(new ComponentsFieldProperties { IsRequired = true }, true);

            await sut.ValidateAsync(CreateValue(1, id.ToString(), "component-field", null), errors, components: components);

            errors.Should().BeEquivalentTo(
                new[] { "[1].component-field: Field is required." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_not_valid()
        {
            var (_, sut, components) = Field(new ComponentsFieldProperties());

            await sut.ValidateAsync(JsonValue.Create("Invalid"), errors, components: components);

            errors.Should().BeEquivalentTo(
                new[] { "Invalid json type, expected array of objects." });
        }

        [Fact]
        public async Task Should_add_error_if_component_is_not_valid()
        {
            var (_, sut, components) = Field(new ComponentsFieldProperties());

            await sut.ValidateAsync(JsonValue.Array(JsonValue.Create("Invalid")), errors, components: components);

            errors.Should().BeEquivalentTo(
                new[] { "Invalid json object, expected object with 'schemaId' field." });
        }

        [Fact]
        public async Task Should_add_error_if_component_has_no_discriminator()
        {
            var (_, sut, components) = Field(new ComponentsFieldProperties());

            await sut.ValidateAsync(CreateValue(1, null, "field", 1), errors, components: components);

            errors.Should().BeEquivalentTo(
                new[] { "Invalid component. No 'schemaId' field found." });
        }

        [Fact]
        public async Task Should_add_error_if_value_has_invalid_discriminator()
        {
            var (_, sut, components) = Field(new ComponentsFieldProperties());

            await sut.ValidateAsync(CreateValue(1, "invalid", "field", 1), errors, components: components);

            errors.Should().BeEquivalentTo(
                new[] { "Invalid component. Cannot find schema." });
        }

        [Fact]
        public async Task Should_add_error_if_value_has_not_enough_components()
        {
            var (id, sut, components) = Field(new ComponentsFieldProperties { MinItems = 3 });

            await sut.ValidateAsync(CreateValue(2, id.ToString(), "component-field", 1), errors, components: components);

            errors.Should().BeEquivalentTo(
                new[] { "Must have at least 3 item(s)." });
        }

        [Fact]
        public async Task Should_add_error_if_value_has_too_much_components()
        {
            var (id, sut, components) = Field(new ComponentsFieldProperties { MaxItems = 1 });

            await sut.ValidateAsync(CreateValue(2, id.ToString(), "component-field", 1), errors, components: components);

            errors.Should().BeEquivalentTo(
                new[] { "Must not have more than 1 item(s)." });
        }

        private static IJsonValue CreateValue(int count, string? type, string key, object? value)
        {
            var result = JsonValue.Array();

            for (var i = 0; i < count; i++)
            {
                var obj = JsonValue.Object();

                if (type != null)
                {
                    obj[Component.Discriminator] = JsonValue.Create(type);
                }

                obj.Add(key, value);

                result.Add(obj);
            }

            return result;
        }

        private static (DomainId, RootField<ComponentsFieldProperties>, ResolvedComponents) Field(ComponentsFieldProperties properties, bool isRequired = false)
        {
            var schema =
                new Schema("my-component")
                    .AddNumber(1, "component-field", Partitioning.Invariant,
                        new NumberFieldProperties { IsRequired = isRequired });

            var id = DomainId.NewGuid();

            var field = Fields.Components(1, "my-components", Partitioning.Invariant, properties);

            var components = new ResolvedComponents(new Dictionary<DomainId, Schema>
            {
                [id] = schema
            });

            return (id, field, components);
        }
    }
}
