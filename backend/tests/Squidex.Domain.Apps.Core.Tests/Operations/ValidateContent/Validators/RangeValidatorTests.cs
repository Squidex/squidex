// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators
{
    public class RangeValidatorTests : IClassFixture<TranslationsFixture>
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public async Task Should_not_add_error_if_value_is_null()
        {
            var sut = new RangeValidator<int>(100, 200);

            await sut.ValidateAsync(null, errors);

            Assert.Empty(errors);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(1000, null)]
        [InlineData(1000, 2000)]
        [InlineData(null, 2000)]
        public async Task Should_not_add_error_if_value_is_within_range(int? min, int? max)
        {
            var sut = new RangeValidator<int>(min, max);

            await sut.ValidateAsync(1500, errors);

            Assert.Empty(errors);
        }

        [Theory]
        [InlineData(20, 10)]
        public void Should_throw_error_if_min_greater_than_max(int? min, int? max)
        {
            Assert.Throws<ArgumentException>(() => new RangeValidator<int>(min, max));
        }

        [Fact]
        public async Task Should_add_error_if_value_is_not_equal_to_min_and_max()
        {
            var sut = new RangeValidator<int>(2000, 2000);

            await sut.ValidateAsync(1500, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must be exactly 2000." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_smaller_than_min()
        {
            var sut = new RangeValidator<int>(2000, null);

            await sut.ValidateAsync(1500, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must be greater or equal to 2000." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_greater_than_max()
        {
            var sut = new RangeValidator<int>(null, 1000);

            await sut.ValidateAsync(1500, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must be less or equal to 1000." });
        }
    }
}
