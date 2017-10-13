// ==========================================================================
//  StringFieldTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public class StringFieldTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = new StringField(1, "my-string", Partitioning.Invariant);

            Assert.Equal("my-string", sut.Name);
        }

        [Fact]
        public void Should_clone_object()
        {
            var sut = new StringField(1, "my-string", Partitioning.Invariant);

            Assert.NotEqual(sut, sut.Enable());
        }

        [Fact]
        public async Task Should_not_add_error_if_string_is_valid()
        {
            var sut = new StringField(1, "my-string", Partitioning.Invariant, new StringFieldProperties { Label = "<FIELD>" });

            await sut.ValidateAsync(CreateValue(null), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_errors_if_string_is_required()
        {
            var sut = new StringField(1, "my-string", Partitioning.Invariant, new StringFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(null), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required." });
        }

        [Fact]
        public async Task Should_add_errors_if_string_is_shorter_than_min_length()
        {
            var sut = new StringField(1, "my-string", Partitioning.Invariant, new StringFieldProperties { MinLength = 10 });

            await sut.ValidateAsync(CreateValue("123"), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> must have more than '10' characters." });
        }

        [Fact]
        public async Task Should_add_errors_if_string_is_longer_than_max_length()
        {
            var sut = new StringField(1, "my-string", Partitioning.Invariant, new StringFieldProperties { MaxLength = 5 });

            await sut.ValidateAsync(CreateValue("12345678"), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> must have less than '5' characters." });
        }

        [Fact]
        public async Task Should_add_errors_if_string_not_allowed()
        {
            var sut = new StringField(1, "my-string", Partitioning.Invariant, new StringFieldProperties { AllowedValues = ImmutableList.Create("Foo") });

            await sut.ValidateAsync(CreateValue("Bar"), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is not an allowed value." });
        }

        [Fact]
        public async Task Should_add_errors_if_number_is_not_valid_pattern()
        {
            var sut = new StringField(1, "my-string", Partitioning.Invariant, new StringFieldProperties { Pattern = "[0-9]{3}" });

            await sut.ValidateAsync(CreateValue("abc"), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is not valid." });
        }

        [Fact]
        public async Task Should_add_errors_if_number_is_not_valid_pattern_with_message()
        {
            var sut = new StringField(1, "my-string", Partitioning.Invariant, new StringFieldProperties { Pattern = "[0-9]{3}", PatternMessage = "Custom Error Message." });

            await sut.ValidateAsync(CreateValue("abc"), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "Custom Error Message." });
        }

        private static JValue CreateValue(object v)
        {
            return new JValue(v);
        }
    }
}
