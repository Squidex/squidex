// ==========================================================================
//  TagsFieldTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public class TagsFieldTests
    {
        private readonly List<string> errors = new List<string>();

        [Fact]
        public void Should_instantiate_field()
        {
            var sut = new TagsField(1, "my-tags", Partitioning.Invariant);

            Assert.Equal("my-tags", sut.Name);
        }

        [Fact]
        public void Should_clone_object()
        {
            var sut = new TagsField(1, "my-tags", Partitioning.Invariant);

            Assert.NotEqual(sut, sut.Enable());
        }

        [Fact]
        public async Task Should_not_add_error_if_tags_are_valid()
        {
            var referenceId = Guid.NewGuid();

            var sut = new TagsField(1, "my-tags", Partitioning.Invariant);

            await sut.ValidateAsync(CreateValue(referenceId), errors, ValidationTestExtensions.ValidContext);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_tags_are_null_and_valid()
        {
            var sut = new TagsField(1, "my-tags", Partitioning.Invariant);

            await sut.ValidateAsync(CreateValue(null), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_errors_if_tags_are_required_and_null()
        {
            var sut = new TagsField(1, "my-tags", Partitioning.Invariant, new TagsFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(null), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required." });
        }

        [Fact]
        public async Task Should_add_errors_if_tags_are_required_and_empty()
        {
            var sut = new TagsField(1, "my-tags", Partitioning.Invariant, new TagsFieldProperties { IsRequired = true });

            await sut.ValidateAsync(CreateValue(), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required." });
        }

        [Fact]
        public async Task Should_add_errors_if_value_is_not_valid()
        {
            var sut = new TagsField(1, "my-tags", Partitioning.Invariant);

            await sut.ValidateAsync("invalid", errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is not a valid value." });
        }

        [Fact]
        public async Task Should_add_errors_if_value_has_not_enough_items()
        {
            var sut = new TagsField(1, "my-tags", Partitioning.Invariant, new TagsFieldProperties { MinItems = 3 });

            await sut.ValidateAsync(CreateValue(Guid.NewGuid(), Guid.NewGuid()), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> must have at least 3 item(s)." });
        }

        [Fact]
        public async Task Should_add_errors_if_value_has_too_much_items()
        {
            var sut = new TagsField(1, "my-tags", Partitioning.Invariant, new TagsFieldProperties { MaxItems = 1 });

            await sut.ValidateAsync(CreateValue(Guid.NewGuid(), Guid.NewGuid()), errors);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> must have not more than 1 item(s)." });
        }

        private static JToken CreateValue(params Guid[] ids)
        {
            return ids == null ? JValue.CreateNull() : (JToken)new JArray(ids.OfType<object>().ToArray());
        }
    }
}
