// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators
{
    public class CollectionValidatorTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public async Task Should_not_add_error_if_value_is_valid()
        {
            var sut = new CollectionValidator(true, 1, 3);

            await sut.ValidateAsync(new List<int> { 1, 2 }, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_optional()
        {
            var sut = new CollectionValidator(true, 1, 3);

            await sut.ValidateOptionalAsync(null, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_value_is_null()
        {
            var sut = new CollectionValidator(true, 1, 3);

            await sut.ValidateAsync(null, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Field is required." });
        }

        [Fact]
        public async Task Should_add_error_if_collection_has_too_few_items()
        {
            var sut = new CollectionValidator(true, 2, 3);

            await sut.ValidateAsync(new List<int> { 1 }, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must have at least 2 item(s)." });
        }

        [Fact]
        public async Task Should_add_error_if_collection_has_too_many_items()
        {
            var sut = new CollectionValidator(true, 2, 3);

            await sut.ValidateAsync(new List<int> { 1, 2, 3, 4 }, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Must have not more than 3 item(s)." });
        }
    }
}
