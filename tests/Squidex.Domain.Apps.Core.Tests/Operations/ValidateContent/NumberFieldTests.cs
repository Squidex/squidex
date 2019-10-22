﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class NumberFieldTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = Field(new NumberFieldProperties());

            Assert.Equal("my-number", sut.Name);
        }

        [Fact]
        public async Task Should_not_add_error_if_number_is_valid()
        {
            var sut = Field(new NumberFieldProperties());

            await sut.ValidateAsync(JsonValue.Null, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_number_is_required()
        {
            var sut = Field(new NumberFieldProperties { IsRequired = true });

            await sut.ValidateAsync(JsonValue.Null, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_error_if_number_is_less_than_min()
        {
            var sut = Field(new NumberFieldProperties { MinValue = 10 });

            await sut.ValidateAsync(CreateValue(5), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must be greater or equal to '10'." });
        }

        [Fact]
        public async Task Should_add_error_if_number_is_greater_than_max()
        {
            var sut = Field(new NumberFieldProperties { MaxValue = 10 });

            await sut.ValidateAsync(CreateValue(20), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must be less or equal to '10'." });
        }

        [Fact]
        public async Task Should_add_error_if_number_is_not_allowed()
        {
            var sut = Field(new NumberFieldProperties { AllowedValues = ReadOnlyCollection.Create(10d) });

            await sut.ValidateAsync(CreateValue(20), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Not an allowed value." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_not_valid()
        {
            var sut = Field(new NumberFieldProperties());

            await sut.ValidateAsync(JsonValue.Create("Invalid"), errors);

            errors.Should().BeEquivalentTo(
                new[] { "Not a valid value." });
        }

        [Fact]
        public async Task Should_add_error_if_unique_constraint_failed()
        {
            var sut = Field(new NumberFieldProperties { IsUnique = true });

            await sut.ValidateAsync(CreateValue(12.5), errors, ValidationTestExtensions.References((Guid.NewGuid(), Guid.NewGuid())));

            errors.Should().BeEquivalentTo(
                new[] { "Another content with the same value exists." });
        }

        private static IJsonValue CreateValue(double v)
        {
            return JsonValue.Create(v);
        }

        private static RootField<NumberFieldProperties> Field(NumberFieldProperties properties)
        {
            return Fields.Number(1, "my-number", Partitioning.Invariant, properties);
        }
    }
}
