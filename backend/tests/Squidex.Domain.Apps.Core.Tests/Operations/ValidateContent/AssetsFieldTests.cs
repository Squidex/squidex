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
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class AssetsFieldTests : IClassFixture<TranslationsFixture>
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = Field(new AssetsFieldProperties());

            Assert.Equal("my-assets", sut.Name);
        }

        [Fact]
        public async Task Should_not_add_error_if_assets_are_null_and_valid()
        {
            var sut = Field(new AssetsFieldProperties());

            await sut.ValidateAsync(CreateValue(null), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_number_of_assets_is_equal_to_min_and_max_items()
        {
            var sut = Field(new AssetsFieldProperties { MinItems = 2, MaxItems = 2 });

            await sut.ValidateAsync(CreateValue(Guid.NewGuid(), Guid.NewGuid()), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_duplicate_values_are_ignored()
        {
            var sut = Field(new AssetsFieldProperties { AllowDuplicates = true });

            await sut.ValidateAsync(CreateValue(Guid.NewGuid(), Guid.NewGuid()), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_assets_are_required_and_null()
        {
            var sut = Field(new AssetsFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(null), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_error_if_assets_are_required_and_empty()
        {
            var sut = Field(new AssetsFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_error_if_value_has_not_enough_items()
        {
            var sut = Field(new AssetsFieldProperties { MinItems = 3 });

            await sut.ValidateAsync(CreateValue(Guid.NewGuid(), Guid.NewGuid()), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must have at least 3 item(s)." });
        }

        [Fact]
        public async Task Should_add_error_if_value_has_too_much_items()
        {
            var sut = Field(new AssetsFieldProperties { MaxItems = 1 });

            await sut.ValidateAsync(CreateValue(Guid.NewGuid(), Guid.NewGuid()), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must not have more than 1 item(s)." });
        }

        [Fact]
        public async Task Should_add_error_if_values_contains_duplicate()
        {
            var sut = Field(new AssetsFieldProperties());

            var id = Guid.NewGuid();

            await sut.ValidateAsync(CreateValue(id, id), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must not contain duplicate values." });
        }

        private static IJsonValue CreateValue(params Guid[]? ids)
        {
            return ids == null ? JsonValue.Null : JsonValue.Array(ids.Select(x => (object)x.ToString()).ToArray());
        }

        private static RootField<AssetsFieldProperties> Field(AssetsFieldProperties properties)
        {
            return Fields.Assets(1, "my-assets", Partitioning.Invariant, properties);
        }
    }
}
