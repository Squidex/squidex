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
    public class UniqueValuesValidatorTests : IClassFixture<TranslationsFixture>
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public async Task Should_not_add_error_if_value_is_null()
        {
            var sut = new UniqueValuesValidator<int>();

            await sut.ValidateAsync(null, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_value_is_not_collection()
        {
            var sut = new UniqueValuesValidator<int>();

            await sut.ValidateAsync("value", errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_array_contains_no_duplicates()
        {
            var sut = new UniqueValuesValidator<int>();

            await sut.ValidateAsync(new[] { 1, 2, 3 }, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_array_contains_duplicates()
        {
            var sut = new UniqueValuesValidator<int>();

            await sut.ValidateAsync(new[] { 1, 2, 2, 3 }, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must not contain duplicate values." });
        }
    }
}
