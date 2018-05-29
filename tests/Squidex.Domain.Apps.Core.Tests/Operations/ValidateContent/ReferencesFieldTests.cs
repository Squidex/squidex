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
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class ReferencesFieldTests
    {
        private readonly List<string> errors = new List<string>();
        private readonly Guid schemaId = Guid.NewGuid();

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

            await sut.ValidateAsync(CreateValue(Guid.NewGuid()), errors, ValidationTestExtensions.ValidContext);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_references_are_null_and_valid()
        {
            var sut = Field(new ReferencesFieldProperties());

            await sut.ValidateAsync(CreateValue(null), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_errors_if_references_are_required_and_null()
        {
            var sut = Field(new ReferencesFieldProperties { SchemaId = schemaId, IsRequired = true });

            await sut.ValidateAsync(CreateValue(null), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_errors_if_references_are_required_and_empty()
        {
            var sut = Field(new ReferencesFieldProperties { SchemaId = schemaId, IsRequired = true });

            await sut.ValidateAsync(CreateValue(), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_errors_if_value_is_not_valid()
        {
            var sut = Field(new ReferencesFieldProperties());

            await sut.ValidateAsync("invalid", errors);

            errors.ShouldBeEquivalentTo(
                new[] { "Not a valid value." });
        }

        [Fact]
        public async Task Should_add_errors_if_value_has_not_enough_items()
        {
            var sut = Field(new ReferencesFieldProperties { SchemaId = schemaId, MinItems = 3 });

            await sut.ValidateAsync(CreateValue(Guid.NewGuid(), Guid.NewGuid()), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "Must have at least 3 item(s)." });
        }

        [Fact]
        public async Task Should_add_errors_if_value_has_too_much_items()
        {
            var sut = Field(new ReferencesFieldProperties { SchemaId = schemaId, MaxItems = 1 });

            await sut.ValidateAsync(CreateValue(Guid.NewGuid(), Guid.NewGuid()), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "Must have not more than 1 item(s)." });
        }

        [Fact]
        public async Task Should_add_errors_if_reference_are_not_valid()
        {
            var referenceId = Guid.NewGuid();

            var sut = Field(new ReferencesFieldProperties { SchemaId = schemaId });

            await sut.ValidateAsync(CreateValue(referenceId), errors, ValidationTestExtensions.InvalidReferences(referenceId));

            errors.ShouldBeEquivalentTo(
                new[] { $"Contains invalid reference '{referenceId}'." });
        }

        private static JToken CreateValue(params Guid[] ids)
        {
            return ids == null ? JValue.CreateNull() : (JToken)new JArray(ids.OfType<object>().ToArray());
        }

        private static RootField<ReferencesFieldProperties> Field(ReferencesFieldProperties properties)
        {
            return Fields.References(1, "my-refs", Partitioning.Invariant, properties);
        }
    }
}
