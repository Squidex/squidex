// ==========================================================================
//  StringLengthValidatorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Squidex.Domain.Apps.Core.Schemas.Validators
{
    public class StringLengthValidatorTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public async Task Should_not_add_error_if_value_is_null()
        {
            var sut = new StringLengthValidator(100, 200);

            await sut.ValidateAsync(null, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_value_is_empty()
        {
            var sut = new StringLengthValidator(100, 200);

            await sut.ValidateAsync(string.Empty, errors);

            Assert.Empty(errors);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(1000, null)]
        [InlineData(1000, 2000)]
        [InlineData(null, 2000)]
        public async Task Should_not_add_error_if_value_is_within_range(int? min, int? max)
        {
            var sut = new StringLengthValidator(min, max);

            await sut.ValidateAsync(CreateString(1500), errors);

            Assert.Empty(errors);
        }

        [Theory]
        [InlineData(20, 10)]
        [InlineData(10, 10)]
        public void Should_throw_error_if_min_greater_than_max(int? min, int? max)
        {
            Assert.Throws<ArgumentException>(() => new StringLengthValidator(min, max));
        }

        [Fact]
        public async Task Should_add_error_if_value_is_smaller_than_min()
        {
            var sut = new StringLengthValidator(2000, null);

            await sut.ValidateAsync(CreateString(1500), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> must have more than '2000' characters." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_greater_than_max()
        {
            var sut = new StringLengthValidator(null, 1000);

            await sut.ValidateAsync(CreateString(1500), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> must have less than '1000' characters." });
        }

        private static string CreateString(int size)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < size; i++)
            {
                sb.Append("x");
            }

            return sb.ToString();
        }
    }
}
