// ==========================================================================
//  BooleanFieldTests.cs
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

namespace Squidex.Domain.Apps.Core.Schemas
{
    public class BooleanFieldTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = new BooleanField(1, "my-bolean", Partitioning.Invariant);

            Assert.Equal("my-bolean", sut.Name);
        }

        [Fact]
        public void Should_clone_object()
        {
            var sut = new BooleanField(1, "name", Partitioning.Invariant);

            Assert.NotEqual(sut, sut.Enable());
        }

        [Fact]
        public async Task Should_not_add_error_if_null_boolean_is_valid()
        {
            var sut = new BooleanField(1, "my-bolean", Partitioning.Invariant);

            await sut.ValidateAsync(CreateValue(null), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_boolean_is_valid()
        {
            var sut = new BooleanField(1, "my-bolean", Partitioning.Invariant);

            await sut.ValidateAsync(CreateValue(true), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_errors_if_boolean_is_required()
        {
            var sut = new BooleanField(1, "my-bolean", Partitioning.Invariant, new BooleanFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(null), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required." });
        }

        [Fact]
        public async Task Should_add_errors_if_value_is_not_valid()
        {
            var sut = new BooleanField(1, "my-bolean", Partitioning.Invariant);

            await sut.ValidateAsync(CreateValue("Invalid"), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is not a valid value." });
        }

        private static JValue CreateValue(object v)
        {
            return new JValue(v);
        }
    }
}
