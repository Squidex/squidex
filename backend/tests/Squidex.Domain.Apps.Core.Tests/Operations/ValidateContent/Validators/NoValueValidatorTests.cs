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
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators
{
    public class NoValueValidatorTests : IClassFixture<TranslationsFixture>
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public async Task Should_not_add_error_if_value_is_undefined()
        {
            var sut = NoValueValidator.Instance;

            await sut.ValidateAsync(Undefined.Value, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_value_is_json_null()
        {
            var sut = NoValueValidator.Instance;

            await sut.ValidateAsync(JsonValue.Null, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Value must not be defined." });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_valid()
        {
            var sut = NoValueValidator.Instance;

            await sut.ValidateAsync(JsonValue.True, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Value must not be defined." });
        }
    }
}
