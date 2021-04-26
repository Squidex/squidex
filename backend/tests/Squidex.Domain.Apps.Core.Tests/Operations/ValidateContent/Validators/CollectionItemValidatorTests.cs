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
    public class CollectionItemValidatorTests : IClassFixture<TranslationsFixture>
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public async Task Should_not_add_error_if_value_is_wrong_type()
        {
            var sut = new CollectionItemValidator(new RangeValidator<int>(2, 4));

            await sut.ValidateAsync(true, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_all_values_are_valid()
        {
            var sut = new CollectionItemValidator(new RangeValidator<int>(2, 4));

            await sut.ValidateAsync(new List<int> { 2, 3, 4 }, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_at_least_one_item_is_not_valid()
        {
            var sut = new CollectionItemValidator(new RangeValidator<int>(2, 4));

            await sut.ValidateAsync(new List<int> { 2, 1, 4, 5 }, errors);

            errors.Should().BeEquivalentTo(
                new[]
                {
                    "[2]: Must be between 2 and 4.",
                    "[4]: Must be between 2 and 4."
                });
        }
    }
}
