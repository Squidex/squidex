// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class UIFieldTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = Field(new UIFieldProperties());

            Assert.Equal("my-ui", sut.Name);
        }

        [Fact]
        public async Task Should_not_add_error_if_value_is_null()
        {
            var sut = Field(new UIFieldProperties());

            await sut.ValidateAsync(null, errors, ValidationTestExtensions.ValidContext);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_value_is_json_null()
        {
            var sut = Field(new UIFieldProperties());

            await sut.ValidateAsync(JsonValue.Null, errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_value_is_valid()
        {
            var sut = Field(new UIFieldProperties { IsRequired = true });

            await sut.ValidateAsync(JsonValue.True, errors);

            errors.Should().BeEquivalentTo(
                new[] { "Value must not be defined." });
        }

        private static RootField<UIFieldProperties> Field(UIFieldProperties properties)
        {
            return Fields.UI(1, "my-ui", Partitioning.Invariant, properties);
        }
    }
}
