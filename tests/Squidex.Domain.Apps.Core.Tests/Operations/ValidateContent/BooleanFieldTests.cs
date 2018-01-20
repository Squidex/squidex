// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class BooleanFieldTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = Field(new BooleanFieldProperties());

            Assert.Equal("my-boolean", sut.Name);
        }

        [Fact]
        public async Task Should_not_add_error_if_null_boolean_is_valid()
        {
            var sut = Field(new BooleanFieldProperties());

            await sut.ValidateAsync(CreateValue(null), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_boolean_is_valid()
        {
            var sut = Field(new BooleanFieldProperties());

            await sut.ValidateAsync(CreateValue(true), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_errors_if_boolean_is_required()
        {
            var sut = Field(new BooleanFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(null), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required." });
        }

        [Fact]
        public async Task Should_add_errors_if_value_is_not_valid()
        {
            var sut = Field(new BooleanFieldProperties());

            await sut.ValidateAsync(CreateValue("Invalid"), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is not a valid value." });
        }

        private static JValue CreateValue(object v)
        {
            return new JValue(v);
        }

        private static BooleanField Field(BooleanFieldProperties properties)
        {
            return new BooleanField(1, "my-boolean", Partitioning.Invariant, properties);
        }
    }
}
