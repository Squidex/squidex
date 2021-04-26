// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators
{
    public class RequiredStringValidatorTests : IClassFixture<TranslationsFixture>
    {
        private readonly List<string> errors = new List<string>();

        [Theory]
        [InlineData("MyString")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  ")]
        public async Task Should_not_add_error_if_value_is_valid(string value)
        {
            var sut = new RequiredStringValidator();

            await sut.ValidateAsync(value, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_optional()
        {
            var sut = new RequiredStringValidator();

            await sut.ValidateAsync(string.Empty, errors, updater: c => c.Optional(true));

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_value_is_another_type()
        {
            var sut = new RequiredStringValidator();

            await sut.ValidateAsync(true, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_empty_strings_are_not_allowed()
        {
            var sut = new RequiredStringValidator(true);

            await sut.ValidateAsync(string.Empty, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_null()
        {
            var sut = new RequiredStringValidator();

            await sut.ValidateAsync(null, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }
    }
}
