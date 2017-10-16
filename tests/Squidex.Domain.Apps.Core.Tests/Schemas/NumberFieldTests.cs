// ==========================================================================
//  NumberFieldTests.cs
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
    public class NumberFieldTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = new NumberField(1, "my-number", Partitioning.Invariant);

            Assert.Equal("my-number", sut.Name);
        }

        [Fact]
        public void Should_clone_object()
        {
            var sut = new NumberField(1, "my-number", Partitioning.Invariant);

            Assert.NotEqual(sut, sut.Enable());
        }

        [Fact]
        public async Task Should_not_add_error_if_number_is_valid()
        {
            var sut = new NumberField(1, "my-number", Partitioning.Invariant);

            await sut.ValidateAsync(CreateValue(null), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_errors_if_number_is_required()
        {
            var sut = new NumberField(1, "my-number", Partitioning.Invariant, new NumberFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(null), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required." });
        }

        [Fact]
        public async Task Should_add_errors_if_number_is_less_than_min()
        {
            var sut = new NumberField(1, "my-number", Partitioning.Invariant, new NumberFieldProperties { MinValue = 10 });

            await sut.ValidateAsync(CreateValue(5), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> must be greater or equals than '10'." });
        }

        [Fact]
        public async Task Should_add_errors_if_number_is_greater_than_max()
        {
            var sut = new NumberField(1, "my-number", Partitioning.Invariant, new NumberFieldProperties { MaxValue = 10 });

            await sut.ValidateAsync(CreateValue(20), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> must be less or equals than '10'." });
        }

        [Fact]
        public async Task Should_add_errors_if_number_is_not_allowed()
        {
            var sut = new NumberField(1, "my-number", Partitioning.Invariant, new NumberFieldProperties { AllowedValues = ImmutableList.Create(10d) });

            await sut.ValidateAsync(CreateValue(20), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is not an allowed value." });
        }

        [Fact]
        public async Task Should_add_errors_if_value_is_not_valid()
        {
            var sut = new NumberField(1, "my-number", Partitioning.Invariant);

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
