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
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators
{
    public class UniqueValidatorTests : IClassFixture<TranslationsFixture>
    {
        private readonly DomainId schemaId = DomainId.NewGuid();
        private readonly List<string> errors = new List<string>();

        [Fact]
        public async Task Should_add_error_if_string_value_not_found()
        {
            var filter = string.Empty;

            var sut = new UniqueValidator(Check(DomainId.NewGuid(), f => filter = f));

            await sut.ValidateAsync("hi", errors, updater: c => c.Nested("property").Nested("iv"));

            errors.Should().BeEquivalentTo(
                new[] { "property: Another content with the same value exists." });

            Assert.Equal("Data.property.iv == 'hi'", filter);
        }

        [Fact]
        public async Task Should_add_error_if_double_value_not_found()
        {
            var filter = string.Empty;

            var sut = new UniqueValidator(Check(DomainId.NewGuid(), f => filter = f));

            await sut.ValidateAsync(12.5, errors, updater: c => c.Nested("property").Nested("iv"));

            errors.Should().BeEquivalentTo(
                new[] { "property: Another content with the same value exists." });

            Assert.Equal("Data.property.iv == 12.5", filter);
        }

        [Fact]
        public async Task Should_not_add_error_if_string_value_not_found_but_in_optimized_mode()
        {
            var sut = new UniqueValidator(Check(DomainId.NewGuid()));

            await sut.ValidateAsync(null, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_string_value_found_with_same_content_id()
        {
            var ctx = ValidationTestExtensions.CreateContext();

            var sut = new UniqueValidator(Check(ctx.ContentId));

            await sut.ValidateAsync("hi", ctx, ValidationTestExtensions.CreateFormatter(errors));

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_double_value_found_with_same_content_id()
        {
            var ctx = ValidationTestExtensions.CreateContext();

            var sut = new UniqueValidator(Check(ctx.ContentId));

            await sut.ValidateAsync(12.5, ctx, ValidationTestExtensions.CreateFormatter(errors));

            Assert.Empty(errors);
        }

        private CheckUniqueness Check(DomainId id, Action<string>? filter = null)
        {
            return filterNode =>
            {
                filter?.Invoke(filterNode.ToString());

                return Task.FromResult<IReadOnlyList<(DomainId, DomainId)>>(new List<(DomainId, DomainId)> { (schemaId, id) });
            };
        }
    }
}
