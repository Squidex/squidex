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
    public class CollectionValidatorTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public async Task Should_not_add_error_if_value_is_valid()
        {
            var sut = new CollectionValidator<int>(true, 1, 3);

            await sut.ValidateAsync(new List<int> { 1, 2 }, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_optional()
        {
            var sut = new CollectionValidator<int>(true, 1, 3);

            await sut.ValidateOptionalAsync(null, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_value_is_null()
        {
            var sut = new CollectionValidator<int>(true, 1, 3);

            await sut.ValidateAsync(null, errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required." });
        }

        [Fact]
        public async Task Should_add_error_if_collection_has_too_few_items()
        {
            var sut = new CollectionValidator<int>(true, 2, 3);

            await sut.ValidateAsync(new List<int> { 1 }, errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> must have at least 2 item(s)." });
        }

        [Fact]
        public async Task Should_add_error_if_collection_has_too_many_items()
        {
            var sut = new CollectionValidator<int>(true, 2, 3);

            await sut.ValidateAsync(new List<int> { 1, 2, 3, 4 }, errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> must have not more than 3 item(s)." });
        }
    }
}
