// ==========================================================================
//  RequiredValidatorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Core.Schemas.Validators;
using Xunit;
using FluentAssertions;

namespace Squidex.Core.Tests.Schemas.Validators
{
    public sealed class RequiredValidatorTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public async Task Should_not_add_error_if_object_is_valid()
        {
            var sut = new RequiredValidator();

            await sut.ValidateAsync(true, errors);

            Assert.Equal(0, errors.Count);
        }

        [Fact]
        public async Task Should_not_add_error_for_empty_string()
        {
            var sut = new RequiredValidator();

            await sut.ValidateAsync(string.Empty, errors);

            Assert.Equal(0, errors.Count);
        }

        [Fact]
        public async Task Should_add_error_if_object_is_null()
        {
            var sut = new RequiredValidator();

            await sut.ValidateAsync(null, errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required" });
        }
    }
}
