// ==========================================================================
//  CollectionValidatorTests.cs
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
    public class CollectionItemValidatorTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public async Task Should_not_add_error_if_value_is_wrong_type()
        {
            var sut = new CollectionItemValidator<int>(new RangeValidator<int>(2, 4));

            await sut.ValidateAsync(true, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_all_values_are_valid()
        {
            var sut = new CollectionItemValidator<int>(new RangeValidator<int>(2, 4));

            await sut.ValidateAsync(new List<int> { 2, 3, 4 }, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_at_least_one_item_is_not_valid()
        {
            var sut = new CollectionItemValidator<int>(new RangeValidator<int>(2, 4));

            await sut.ValidateAsync(new List<int> { 2, 1, 4, 5 }, errors);

            errors.ShouldBeEquivalentTo(
                new[]
                {
                    "<FIELD> item #2 must be greater or equals than '2'.",
                    "<FIELD> item #4 must be less or equals than '4'."
                });
        }
    }
}
