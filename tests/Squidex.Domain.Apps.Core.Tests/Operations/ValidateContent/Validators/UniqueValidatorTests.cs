// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators
{
    public class UniqueValidatorTests
    {
        private readonly List<string> errors = new List<string>();
        private readonly Guid contentId = Guid.NewGuid();
        private readonly Guid schemaId = Guid.NewGuid();

        [Fact]
        public async Task Should_add_error_if_string_value_not_found()
        {
            var sut = new UniqueValidator();

            var filter = string.Empty;

            await sut.ValidateAsync("hi", errors, Context(Guid.NewGuid(), f => filter = f));

            errors.Should().BeEquivalentTo(
                new[] { "property: Another content with the same value exists." });

            Assert.Equal("Data.property.iv == 'hi'", filter);
        }

        [Fact]
        public async Task Should_add_error_if_double_value_not_found()
        {
            var sut = new UniqueValidator();

            var filter = string.Empty;

            await sut.ValidateAsync(12.5, errors, Context(Guid.NewGuid(), f => filter = f));

            errors.Should().BeEquivalentTo(
                new[] { "property: Another content with the same value exists." });

            Assert.Equal("Data.property.iv == 12.5", filter);
        }

        [Fact]
        public async Task Should_not_add_error_if_string_value_found()
        {
            var sut = new UniqueValidator();

            var filter = string.Empty;

            await sut.ValidateAsync("hi", errors, Context(contentId, f => filter = f));

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_double_value_found()
        {
            var sut = new UniqueValidator();

            var filter = string.Empty;

            await sut.ValidateAsync(12.5, errors, Context(contentId, f => filter = f));

            Assert.Empty(errors);
        }

        private ValidationContext Context(Guid id, Action<string> filter)
        {
            return new ValidationContext(contentId, schemaId,
                (schema, filterNode) =>
                {
                    filter(filterNode.ToString());

                    return Task.FromResult<IReadOnlyList<Guid>>(new List<Guid> { id });
                },
                ids =>
                {
                    return Task.FromResult<IReadOnlyList<IAssetInfo>>(new List<IAssetInfo>());
                }).Nested("property").Nested("iv");
        }
    }
}
