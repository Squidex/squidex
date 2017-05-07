// ==========================================================================
//  AllowedValuesValidatorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Squidex.Core.Schemas.Validators
{
    public class AllowedValuesValidatorTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public async Task Should_not_error_if_value_null()
        {
            var sut = new AllowedValuesValidator<int>(100, 200);

            await sut.ValidateAsync(null, false, errors.Add);

            Assert.Equal(0, errors.Count);
        }

        [Fact]
        public async Task Should_not_error_if_value_is_allowed()
        {
            var sut = new AllowedValuesValidator<int>(100, 200);

            await sut.ValidateAsync(100, false, errors.Add);

            Assert.Equal(0, errors.Count);
        }

        [Fact]
        public async Task Should_add_error_if_value_is_not_allowed()
        {
            var sut = new AllowedValuesValidator<int>(100, 200);

            await sut.ValidateAsync(50, false, errors.Add);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is not an allowed value" });
        }
    }
}