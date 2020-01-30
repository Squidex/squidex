﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class ArrayFieldTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = Field(new ArrayFieldProperties());

            Assert.Equal("my-array", sut.Name);
        }

        [Fact]
        public async Task Should_not_add_error_if_items_are_valid()
        {
            var sut = Field(new ArrayFieldProperties());

            await sut.ValidateAsync(CreateValue(JsonValue.Object()), errors, ValidationTestExtensions.ValidContext);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_items_are_null_and_valid()
        {
            var sut = Field(new ArrayFieldProperties());

            await sut.ValidateAsync(CreateValue(null), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_number_of_items_is_equal_to_min_and_max_items()
        {
            var sut = Field(new ArrayFieldProperties { MinItems = 2, MaxItems = 2 });

            await sut.ValidateAsync(CreateValue(JsonValue.Object(), JsonValue.Object()), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_items_are_required_and_null()
        {
            var sut = Field(new ArrayFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(null), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_error_if_items_are_required_and_empty()
        {
            var sut = Field(new ArrayFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_not_valid()
        {
            var sut = Field(new ArrayFieldProperties());

            await sut.ValidateAsync(JsonValue.Create("invalid"), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Invalid json type, expected array of objects." });
        }

        [Fact]
        public async Task Should_add_error_if_value_has_not_enough_items()
        {
            var sut = Field(new ArrayFieldProperties { MinItems = 3 });

            await sut.ValidateAsync(CreateValue(JsonValue.Object(), JsonValue.Object()), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must have at least 3 item(s)." });
        }

        [Fact]
        public async Task Should_add_error_if_value_has_too_much_items()
        {
            var sut = Field(new ArrayFieldProperties { MaxItems = 1 });

            await sut.ValidateAsync(CreateValue(JsonValue.Object(), JsonValue.Object()), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must not have more than 1 item(s)." });
        }

        private static IJsonValue CreateValue(params JsonObject[]? ids)
        {
            return ids == null ? JsonValue.Null : JsonValue.Array(ids.OfType<object>().ToArray());
        }

        private static RootField<ArrayFieldProperties> Field(ArrayFieldProperties properties)
        {
            return Fields.Array(1, "my-array", Partitioning.Invariant, properties);
        }
    }
}
