// ==========================================================================
//  JsonFieldTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Squidex.Core.Schemas
{
    public class JsonFieldTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = new JsonField(1, "my-json", new JsonFieldProperties());

            Assert.Equal("my-json", sut.Name);
        }

        [Fact]
        public void Should_clone_object()
        {
            var sut = new JsonField(1, "my-json", new JsonFieldProperties());

            Assert.NotEqual(sut, sut.Enable());
        }

        [Fact]
        public async Task Should_not_add_error_if_json_is_valid()
        {
            var sut = new JsonField(1, "my-json", new JsonFieldProperties());

            await sut.ValidateAsync(CreateValue(new JValue(1)), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_errors_if_json_is_required()
        {
            var sut = new JsonField(1, "my-json", new JsonFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(JValue.CreateNull()), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required" });
        }

        private static JValue CreateValue(JValue v)
        {
            return v;
        }
    }
}
