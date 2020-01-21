// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class ReferencesFieldTests
    {
        private readonly List<string> errors = new List<string>();
        private readonly Guid schemaId = Guid.NewGuid();
        private readonly Guid ref1 = Guid.NewGuid();
        private readonly Guid ref2 = Guid.NewGuid();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = Field(new ReferencesFieldProperties());

            Assert.Equal("my-refs", sut.Name);
        }

        [Fact]
        public async Task Should_not_add_error_if_references_are_valid()
        {
            var sut = Field(new ReferencesFieldProperties());

            await sut.ValidateAsync(CreateValue(ref1), errors, Context());

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_references_are_null_and_valid()
        {
            var sut = Field(new ReferencesFieldProperties());

            await sut.ValidateAsync(CreateValue(null), errors, Context());

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_number_of_references_is_equal_to_min_and_max_items()
        {
            var sut = Field(new ReferencesFieldProperties { MinItems = 2, MaxItems = 2 });

            await sut.ValidateAsync(CreateValue(ref1, ref2), errors, Context());

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_duplicate_values_are_allowed()
        {
            var sut = Field(new ReferencesFieldProperties { MinItems = 2, MaxItems = 2, AllowDuplicates = true });

            await sut.ValidateAsync(CreateValue(ref1, ref1), errors, Context());

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_schemas_not_defined()
        {
            var sut = Field(new ReferencesFieldProperties());

            await sut.ValidateAsync(CreateValue(ref1), errors, ValidationTestExtensions.References((Guid.NewGuid(), ref1)));

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_references_are_required_and_null()
        {
            var sut = Field(new ReferencesFieldProperties { SchemaId = schemaId, IsRequired = true });

            await sut.ValidateAsync(CreateValue(null), errors, Context());

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_error_if_references_are_required_and_empty()
        {
            var sut = Field(new ReferencesFieldProperties { SchemaId = schemaId, IsRequired = true });

            await sut.ValidateAsync(CreateValue(), errors, Context());

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_not_valid()
        {
            var sut = Field(new ReferencesFieldProperties());

            await sut.ValidateAsync(JsonValue.Create("invalid"), errors, Context());

            errors.Should().BeEquivalentTo(
                new[] { "Not a valid value." });
        }

        [Fact]
        public async Task Should_add_error_if_value_has_not_enough_items()
        {
            var sut = Field(new ReferencesFieldProperties { SchemaId = schemaId, MinItems = 3 });

            await sut.ValidateAsync(CreateValue(ref1, ref2), errors, Context());

            errors.Should().BeEquivalentTo(
                new[] { "Must have at least 3 item(s)." });
        }

        [Fact]
        public async Task Should_add_error_if_value_has_too_much_items()
        {
            var sut = Field(new ReferencesFieldProperties { SchemaId = schemaId, MaxItems = 1 });

            await sut.ValidateAsync(CreateValue(ref1, ref2), errors, Context());

            errors.Should().BeEquivalentTo(
                new[] { "Must not have more than 1 item(s)." });
        }

        [Fact]
        public async Task Should_add_error_if_reference_are_not_valid()
        {
            var sut = Field(new ReferencesFieldProperties { SchemaId = schemaId });

            await sut.ValidateAsync(CreateValue(ref1), errors, ValidationTestExtensions.References());

            errors.Should().BeEquivalentTo(
                new[] { $"Contains invalid reference '{ref1}'." });
        }

        [Fact]
        public async Task Should_not_add_error_if_reference_are_not_valid_but_in_optimized_mode()
        {
            var sut = Field(new ReferencesFieldProperties { SchemaId = schemaId });

            await sut.ValidateAsync(CreateValue(ref1), errors, ValidationTestExtensions.References().Optimized());

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_reference_schema_is_not_valid()
        {
            var sut = Field(new ReferencesFieldProperties { SchemaId = schemaId });

            await sut.ValidateAsync(CreateValue(ref1), errors, ValidationTestExtensions.References((Guid.NewGuid(), ref1)));

            errors.Should().BeEquivalentTo(
                new[] { $"Contains reference '{ref1}' to invalid schema." });
        }

        [Fact]
        public async Task Should_add_error_if_reference_contains_duplicate_values()
        {
            var sut = Field(new ReferencesFieldProperties { SchemaId = schemaId });

            await sut.ValidateAsync(CreateValue(ref1, ref1), errors,
                ValidationTestExtensions.References(
                    (schemaId, ref1)));

            errors.Should().BeEquivalentTo(
                new[] { "Must not contain duplicate values." });
        }

        private static IJsonValue CreateValue(params Guid[]? ids)
        {
            return ids == null ? JsonValue.Null : JsonValue.Array(ids.Select(x => (object)x.ToString()).ToArray());
        }

        private ValidationContext Context()
        {
            return ValidationTestExtensions.References(
                (schemaId, ref1),
                (schemaId, ref2));
        }

        private static RootField<ReferencesFieldProperties> Field(ReferencesFieldProperties properties)
        {
            return Fields.References(1, "my-refs", Partitioning.Invariant, properties);
        }
    }
}
