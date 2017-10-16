// ==========================================================================
//  PatternValidatorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Squidex.Domain.Apps.Core.Schemas.Validators
{
    public class PatternValidatorTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public async Task Should_not_add_error_if_value_is_valid()
        {
            var sut = new PatternValidator("[a-z]{3}:[0-9]{2}");

            await sut.ValidateAsync("abc:12", errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_value_is_null()
        {
            var sut = new PatternValidator("[a-z]{3}:[0-9]{2}");

            await sut.ValidateAsync(null, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_value_is_empty()
        {
            var sut = new PatternValidator("[a-z]{3}:[0-9]{2}");

            await sut.ValidateAsync(string.Empty, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_with_default_message_if_value_is_not_valid()
        {
            var sut = new PatternValidator("[a-z]{3}:[0-9]{2}");

            await sut.ValidateAsync("foo", errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is not valid." });
        }

        [Fact]
        public async Task Should_add_error_with_custom_message_if_value_is_not_valid()
        {
            var sut = new PatternValidator("[a-z]{3}:[0-9]{2}", "Custom Error Message.");

            await sut.ValidateAsync("foo", errors);

            errors.ShouldBeEquivalentTo(
                new[] { "Custom Error Message." });
        }
    }
}
