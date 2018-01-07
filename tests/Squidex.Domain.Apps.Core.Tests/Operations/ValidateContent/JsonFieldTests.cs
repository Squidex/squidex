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
    public class JsonFieldTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = Field(new JsonFieldProperties());

            Assert.Equal("my-json", sut.Name);
        }

        [Fact]
        public async Task Should_not_add_error_if_json_is_valid()
        {
            var sut = Field(new JsonFieldProperties());

            await sut.ValidateAsync(CreateValue(new JValue(1)), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_errors_if_json_is_required()
        {
            var sut = Field(new JsonFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(JValue.CreateNull()), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required." });
        }

        private static JValue CreateValue(JValue v)
        {
            return v;
        }

        private static JsonField Field(JsonFieldProperties properties)
        {
            return new JsonField(1, "my-json", Partitioning.Invariant, properties);
        }
    }
}
