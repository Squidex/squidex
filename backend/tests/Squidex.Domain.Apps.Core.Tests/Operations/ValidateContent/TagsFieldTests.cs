// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class TagsFieldTests : IClassFixture<TranslationsFixture>
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = Field(new TagsFieldProperties());

            Assert.Equal("myTags", sut.Name);
        }

        [Fact]
        public async Task Should_not_add_error_if_tags_are_valid()
        {
            var sut = Field(new TagsFieldProperties());

            await sut.ValidateAsync(CreateValue("tag"), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_tags_are_null_and_valid()
        {
            var sut = Field(new TagsFieldProperties());

            await sut.ValidateAsync(CreateValue(null), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_number_of_tags_is_equal_to_min_and_max_items()
        {
            var sut = Field(new TagsFieldProperties { MinItems = 2, MaxItems = 2 });

            await sut.ValidateAsync(CreateValue("tag1", "tag2"), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_tags_are_required_but_null()
        {
            var sut = Field(new TagsFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(null), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_error_if_tags_are_required_but_empty()
        {
            var sut = Field(new TagsFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_error_if_tag_value_is_null()
        {
            var sut = Field(new TagsFieldProperties { IsRequired = true });

            await sut.ValidateAsync(JsonValue.Array(JsonValue.Null), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Invalid json type, expected array of strings." });
        }

        [Fact]
        public async Task Should_add_error_if_tag_value_is_empty()
        {
            var sut = Field(new TagsFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(string.Empty), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Invalid json type, expected array of strings." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_not_valid()
        {
            var sut = Field(new TagsFieldProperties());

            await sut.ValidateAsync(JsonValue.Create("invalid"), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Invalid json type, expected array of strings." });
        }

        [Fact]
        public async Task Should_add_error_if_value_has_not_enough_items()
        {
            var sut = Field(new TagsFieldProperties { MinItems = 3 });

            await sut.ValidateAsync(CreateValue("tag-1", "tag-2"), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must have at least 3 item(s)." });
        }

        [Fact]
        public async Task Should_add_error_if_value_has_too_much_items()
        {
            var sut = Field(new TagsFieldProperties { MaxItems = 1 });

            await sut.ValidateAsync(CreateValue("tag-1", "tag-2"), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must not have more than 1 item(s)." });
        }

        [Fact]
        public async Task Should_add_error_if_value_contains_an_not_allowed_values()
        {
            var sut = Field(new TagsFieldProperties { AllowedValues = ImmutableList.Create("tag-2", "tag-3") });

            await sut.ValidateAsync(CreateValue("tag-1", "tag-2", null), errors);

            errors.Should().BeEquivalentTo(
                new[] { "[1]: Not an allowed value." });
        }

        private static IJsonValue CreateValue(params string?[]? ids)
        {
            return ids == null ? JsonValue.Null : JsonValue.Array(ids.OfType<object>().ToArray());
        }

        private static RootField<TagsFieldProperties> Field(TagsFieldProperties properties)
        {
            return Fields.Tags(1, "myTags", Partitioning.Invariant, properties);
        }
    }
}
