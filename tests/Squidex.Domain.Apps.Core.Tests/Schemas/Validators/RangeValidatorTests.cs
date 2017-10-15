// ==========================================================================
//  RangeValidatorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Squidex.Domain.Apps.Core.Schemas.Validators
{
    public class RangeValidatorTests
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
        [InlineData(10, 10)]
        public void Should_throw_error_if_min_greater_than_max(int? min, int? max)
        {
            Assert.Throws<ArgumentException>(() => new RangeValidator<int>(min, max));
        }

        [Fact]
        public async Task Should_add_error_if_value_is_smaller_than_min()
        {
            var sut = new RangeValidator<int>(2000, null);

            await sut.ValidateAsync(1500, errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> must be greater or equals than '2000'." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_greater_than_max()
        {
            var sut = new RangeValidator<int>(null, 1000);

            await sut.ValidateAsync(1500, errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> must be less or equals than '1000'." });
        }
    }
}
