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
    public class ComponentFieldTests : IClassFixture<TranslationsFixture>
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var (_, sut, _) = Field(new ComponentFieldProperties());

            Assert.Equal("my-component", sut.Name);
        }

        [Fact]
        public async Task Should_not_add_error_if_component_is_null_and_valid()
        {
            var (_, sut, components) = Field(new ComponentFieldProperties());

            await sut.ValidateAsync(null, errors, components: components);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_component_is_valid()
        {
            var (id, sut, components) = Field(new ComponentFieldProperties());

            await sut.ValidateAsync(CreateValue(id.ToString(), "component-field", 1), errors, components: components);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_component_is_required()
        {
            var (_, sut, components) = Field(new ComponentFieldProperties { IsRequired = true });

            await sut.ValidateAsync(null, errors, components: components);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_error_if_component_value_is_required()
        {
            var (id, sut, components) = Field(new ComponentFieldProperties { IsRequired = true }, true);

            await sut.ValidateAsync(CreateValue(id.ToString(), "component-field", null), errors, components: components);

            errors.Should().BeEquivalentTo(
                new[] { "component-field: Field is required." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_not_valid()
        {
            var (_, sut, components) = Field(new ComponentFieldProperties());

            await sut.ValidateAsync(JsonValue.Create("Invalid"), errors, components: components);

            errors.Should().BeEquivalentTo(
                new[] { "Invalid json object, expected object with 'schemaId' field." });
        }

        [Fact]
        public async Task Should_add_error_if_value_has_no_discriminator()
        {
            var (_, sut, components) = Field(new ComponentFieldProperties());

            await sut.ValidateAsync(CreateValue(null, "field", 1), errors, components: components);

            errors.Should().BeEquivalentTo(
                new[] { "Invalid component. No 'schemaId' field found." });
        }

        [Fact]
        public async Task Should_add_error_if_value_has_invalid_discriminator()
        {
            var (_, sut, components) = Field(new ComponentFieldProperties());

            await sut.ValidateAsync(CreateValue("invalid", "field", 1), errors, components: components);

            errors.Should().BeEquivalentTo(
                new[] { "Invalid component. Cannot find schema." });
        }

        private static IJsonValue CreateValue(string? type, string key, object? value)
        {
            var obj = JsonValue.Object();

            if (type != null)
            {
                obj[Component.Discriminator] = JsonValue.Create(type);
            }

            obj.Add(key, value);

            return obj;
        }

        private static (DomainId, RootField<ComponentFieldProperties>, ResolvedComponents) Field(ComponentFieldProperties properties, bool isRequired = false)
        {
            var schema =
                new Schema("my-component")
                    .AddNumber(1, "component-field", Partitioning.Invariant,
                        new NumberFieldProperties { IsRequired = isRequired });

            var id = DomainId.NewGuid();

            var field = Fields.Component(1, "my-component", Partitioning.Invariant, properties);

            var components = new ResolvedComponents(new Dictionary<DomainId, Schema>
            {
                [id] = schema
            });

            return (id, field, components);
        }
    }
}
