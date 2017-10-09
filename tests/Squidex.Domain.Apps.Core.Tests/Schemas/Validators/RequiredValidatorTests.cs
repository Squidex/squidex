// ==========================================================================
//  RequiredValidatorTests.cs
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
    public sealed class RequiredValidatorTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public async Task Should_not_add_error_if_value_is_valid()
        {
            var sut = new RequiredValidator();

            await sut.ValidateAsync(true, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_for_empty_string()
        {
            var sut = new RequiredValidator();

            await sut.ValidateAsync(string.Empty, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_optional()
        {
            var sut = new RequiredValidator();

            await sut.ValidateOptionalAsync(null, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_value_is_null()
        {
            var sut = new RequiredValidator();

            await sut.ValidateAsync(null, errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required" });
        }
    }
}
