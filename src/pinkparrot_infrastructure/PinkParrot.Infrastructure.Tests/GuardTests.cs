// ==========================================================================
//  GuardTests.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using Xunit;

namespace PinkParrot.Infrastructure
{
    public class GuardTest
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_throw_when_target_is_null_for_empty_string(string invalidString)
        {
            Assert.Throws<ArgumentException>(() => Guard.NotNullOrEmpty(invalidString, "parameter"));
        }

        [Fact]
        public void Should_do_nothing_if_target_string_is_valid()
        {
            Guard.NotNullOrEmpty("value", "parameter");
        }

        [Fact]
        public void Should_do_nothing_if_target_is_not_null()
        {
            Guard.NotNull("value", "parameter");
        }

        [Fact]
        public void Should_do_nothing_if_enum_is_valid()
        {
            Guard.Enum(DateTimeKind.Local, "Parameter");
        }

        [Fact]
        public void Should_throw_if_enum_is_not_valid()
        {
            Assert.Throws<ArgumentException>(() => Guard.Enum((DateTimeKind)13, "Parameter"));
        }

        [Fact]
        public void Should_do_nothing_when_guid_is_not_empty()
        {
            Guard.NotEmpty(Guid.NewGuid(), "parameter");
        }

        [Fact]
        public void Should_throw_when_guid_is_empty()
        {
            Assert.Throws<ArgumentException>(() => Guard.NotEmpty(Guid.Empty, "parameter"));
        }

        [Fact]
        public void Should_throw_when_target_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => Guard.NotNull(null, "parameter"));
        }

        [Fact]
        public void Should_throw_when_target_is_null_for_null_string()
        {
            Assert.Throws<ArgumentNullException>(() => Guard.NotNullOrEmpty(null, "parameter"));
        }

        [Fact]
        public void Should_do_nothing_when_target_has_correct_type()
        {
            Guard.HasType<int>(123, "parameter");
        }

        [Fact]
        public void Should_throw_when_target_has_wrong_type()
        {
            Assert.Throws<ArgumentException>(() => Guard.HasType<int>("value", "parameter"));
        }

        [Fact]
        public void Should_throw_when_checking_for_null_and_target_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => Guard.HasType<int>(null, "parameter"));
        }

        [Fact]
        public void Should_do_nothing_when_target_is_not_default_value()
        {
            Guard.NotDefault(Guid.NewGuid(), "parameter");   
        }

        [Fact]
        public void Should_throw_exception_when_value_has_default()
        {
            Assert.Throws<ArgumentException>(() => Guard.NotDefault(Guid.Empty, "parameter"));
            Assert.Throws<ArgumentException>(() => Guard.NotDefault(0, "parameter"));
            Assert.Throws<ArgumentException>(() => Guard.NotDefault((string)null, "parameter"));
            Assert.Throws<ArgumentException>(() => Guard.NotDefault(false, "parameter"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(" Not a Slug ")]
        [InlineData(" not--a--slug ")]
        [InlineData(" not-a-slug ")]
        [InlineData("-not-a-slug-")]
        [InlineData("not$-a-slug")]
        public void Should_throw_exception_for_invalid_slug(string slug)
        {
            Assert.Throws<ArgumentException>(() => Guard.ValidSlug(slug, "slug"));
        }

        [Theory]
        [InlineData("slug")]
        [InlineData("slug23")]
        [InlineData("other-slug")]
        [InlineData("just-another-slug")]
        public void Should_do_nothing_for_valid_slug(string slug)
        {
            Guard.ValidSlug(slug, "parameter");
        }
    }
}