// ==========================================================================
//  GuardTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Xunit;

namespace Squidex.Infrastructure
{
    public sealed class MyValidatableValid : IValidatable
    {
        public void Validate(IList<ValidationError> errors)
        {
        }
    }

    public sealed class MyValidatableInvalid : IValidatable
    {
        public void Validate(IList<ValidationError> errors)
        {
            errors.Add(new ValidationError("error", "error"));
        }
    }

    public class GuardTest
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void NotNullOrEmpty_should_throw_for_empy_strings(string invalidString)
        {
            Assert.Throws<ArgumentException>(() => Guard.NotNullOrEmpty(invalidString, "parameter"));
        }

        [Fact]
        public void NotNullOrEmpty_should_throw_for_null_string()
        {
            Assert.Throws<ArgumentNullException>(() => Guard.NotNullOrEmpty(null, "parameter"));
        }

        [Fact]
        public void NotNullOrEmpty_should_do_nothing_for_vaid_string()
        {
            Guard.NotNullOrEmpty("value", "parameter");
        }

        [Fact]
        public void NotNull_should_throw_for_null_value()
        {
            Assert.Throws<ArgumentNullException>(() => Guard.NotNull(null, "parameter"));
        }

        [Fact]
        public void NotNull_should_do_nothing_for_valid_value()
        {
            Guard.NotNull("value", "parameter");
        }

        [Fact]
        public void Enum_should_throw_for_invalid_enum()
        {
            Assert.Throws<ArgumentException>(() => Guard.Enum((DateTimeKind)13, "Parameter"));
        }

        [Fact]
        public void Enum_should_do_nothing_for_valid_enum()
        {
            Guard.Enum(DateTimeKind.Local, "Parameter");
        }

        [Fact]
        public void NotEmpty_should_throw_for_empty_guid()
        {
            Assert.Throws<ArgumentException>(() => Guard.NotEmpty(Guid.Empty, "parameter"));
        }

        [Fact]
        public void NotEmpty_should_do_nothing_for_valid_guid()
        {
            Guard.NotEmpty(Guid.NewGuid(), "parameter");
        }

        [Fact]
        public void HasType_should_throw_for_other_type()
        {
            Assert.Throws<ArgumentException>(() => Guard.HasType<int>("value", "parameter"));
        }

        [Fact]
        public void HasType_should_do_nothing_for_null_value()
        {
            Guard.HasType<int>(null, "parameter");
        }

        [Fact]
        public void HasType_should_do_nothing_for_correct_type()
        {
            Guard.HasType<int>(123, "parameter");
        }

        [Fact]
        public void HasType_nongeneric_should_throw_for_other_type()
        {
            Assert.Throws<ArgumentException>(() => Guard.HasType("value", typeof(int), "parameter"));
        }

        [Fact]
        public void HasType_nongeneric_should_do_nothing_for_null_value()
        {
            Guard.HasType(null, typeof(int), "parameter");
        }

        [Fact]
        public void HasType_nongeneric_should_do_nothing_for_correct_type()
        {
            Guard.HasType(123, typeof(int), "parameter");
        }

        [Fact]
        public void HasType_nongeneric_should_do_nothing_for_null_type()
        {
            Guard.HasType(123, null, "parameter");
        }

        [Fact]
        public void NotDefault_should_throw_for_default_values()
        {
            Assert.Throws<ArgumentException>(() => Guard.NotDefault(Guid.Empty, "parameter"));
            Assert.Throws<ArgumentException>(() => Guard.NotDefault(0, "parameter"));
            Assert.Throws<ArgumentException>(() => Guard.NotDefault((string)null, "parameter"));
            Assert.Throws<ArgumentException>(() => Guard.NotDefault(false, "parameter"));
        }

        [Fact]
        public void NotDefault_should_do_nothing_for_non_default_value()
        {
            Guard.NotDefault(Guid.NewGuid(), "parameter");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(" Not a Slug ")]
        [InlineData(" not--a--slug ")]
        [InlineData(" not-a-slug ")]
        [InlineData("-not-a-slug-")]
        [InlineData("not$-a-slug")]
        [InlineData("not-a-Slug")]
        public void ValidSlug_should_throw_for_invalid_slugs(string slug)
        {
            Assert.Throws<ArgumentException>(() => Guard.ValidSlug(slug, "parameter"));
        }

        [Theory]
        [InlineData("slug")]
        [InlineData("slug23")]
        [InlineData("other-slug")]
        [InlineData("just-another-slug")]
        public void ValidSlug_should_do_nothing_for_valid_slugs(string slug)
        {
            Guard.ValidSlug(slug, "parameter");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(" Not a Property ")]
        [InlineData(" not--a--property ")]
        [InlineData(" not-a-property ")]
        [InlineData("-not-a-property-")]
        [InlineData("not$-a-property")]
        public void ValidPropertyName_should_throw_for_invalid_slugs(string slug)
        {
            Assert.Throws<ArgumentException>(() => Guard.ValidPropertyName(slug, "property"));
        }

        [Theory]
        [InlineData("property")]
        [InlineData("property23")]
        [InlineData("other-property")]
        [InlineData("other-Property")]
        [InlineData("otherProperty")]
        [InlineData("just-another-property")]
        [InlineData("just-Another-Property")]
        [InlineData("justAnotherProperty")]
        public void ValidPropertyName_should_do_nothing_for_valid_slugs(string property)
        {
            Guard.ValidPropertyName(property, "parameter");
        }

        [Theory]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void ValidNumber_should_throw_for_invalid_doubles(double value)
        {
            Assert.Throws<ArgumentException>(() => Guard.ValidNumber(value, "parameter"));
        }

        [Theory]
        [InlineData(0d)]
        [InlineData(-1000d)]
        [InlineData(1000d)]
        public void ValidNumber_do_nothing_for_valid_double(double value)
        {
            Guard.ValidNumber(value, "parameter");
        }

        [Theory]
        [InlineData(float.PositiveInfinity)]
        [InlineData(float.NegativeInfinity)]
        [InlineData(float.NaN)]
        public void ValidNumber_should_throw_for_invalid_float(float value)
        {
            Assert.Throws<ArgumentException>(() => Guard.ValidNumber(value, "parameter"));
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(-1000f)]
        [InlineData(1000f)]
        public void ValidNumber_do_nothing_for_valid_float(float value)
        {
            Guard.ValidNumber(value, "parameter");
        }

        [Theory]
        [InlineData(4)]
        [InlineData(104)]
        public void Between_should_throw_for_values_outside_of_range(int value)
        {
            Assert.Throws<ArgumentException>(() => Guard.Between(value, 10, 100, "parameter"));
        }

        [Theory]
        [InlineData(10)]
        [InlineData(55)]
        [InlineData(100)]
        public void Between_should_do_nothing_for_values_in_range(int value)
        {
            Guard.Between(value, 10, 100, "parameter");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        public void GreaterThan_should_throw_for_smaller_values(int value)
        {
            Assert.Throws<ArgumentException>(() => Guard.GreaterThan(value, 100, "parameter"));
        }

        [Theory]
        [InlineData(101)]
        [InlineData(200)]
        public void GreaterThan_should_do_nothing_for_greater_values(int value)
        {
            Guard.GreaterThan(value, 100, "parameter");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(99)]
        public void GreaterEquals_should_throw_for_smaller_values(int value)
        {
            Assert.Throws<ArgumentException>(() => Guard.GreaterEquals(value, 100, "parameter"));
        }

        [Theory]
        [InlineData(100)]
        [InlineData(200)]
        public void GreaterEquals_should_do_nothing_for_greater_values(int value)
        {
            Guard.GreaterEquals(value, 100, "parameter");
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(100)]
        public void LessThan_should_throw_for_greater_values(int value)
        {
            Assert.Throws<ArgumentException>(() => Guard.LessThan(value, 100, "parameter"));
        }

        [Theory]
        [InlineData(99)]
        [InlineData(50)]
        public void LessThan_should_do_nothing_for_smaller_values(int value)
        {
            Guard.LessThan(value, 100, "parameter");
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(101)]
        public void LessEquals_should_throw_for_greater_values(int value)
        {
            Assert.Throws<ArgumentException>(() => Guard.LessEquals(value, 100, "parameter"));
        }

        [Theory]
        [InlineData(100)]
        [InlineData(50)]
        public void LessEquals_should_do_nothing_for_smaller_values(int value)
        {
            Guard.LessEquals(value, 100, "parameter");
        }

        [Fact]
        public void NotEmpty_should_throw_for_empty_collection()
        {
            Assert.Throws<ArgumentException>(() => Guard.NotEmpty(new int[0], "parameter"));
        }

        [Fact]
        public void NotEmpty_should_throw_for_null_collection()
        {
            Assert.Throws<ArgumentNullException>(() => Guard.NotEmpty((int[])null, "parameter"));
        }

        [Fact]
        public void NotEmpty_should_do_nothing_for_value_collection()
        {
            Guard.NotEmpty(new[] { 1, 2, 3 }, "parameter");
        }

        [Fact]
        public void ValidFileName_should_throw_for_invalid_file_name()
        {
            Assert.Throws<ArgumentException>(() => Guard.ValidFileName("File/Name", "Parameter"));
        }

        [Fact]
        public void ValidFileName_should_throw_for_null_file_name()
        {
            Assert.Throws<ArgumentNullException>(() => Guard.ValidFileName(null, "Parameter"));
        }

        [Fact]
        public void ValidFileName_should_do_nothing_for_valid_file_name()
        {
            Guard.ValidFileName("FileName", "Parameter");
        }

        [Fact]
        public void Valid_should_throw_exception_if_null()
        {
            Assert.Throws<ArgumentNullException>(() => Guard.Valid(null, "Parameter", () => "Message"));
        }

        [Fact]
        public void Valid_should_throw_exception_if_invalid()
        {
            Assert.Throws<ValidationException>(() => Guard.Valid(new MyValidatableInvalid(), "Parameter", () => "Message"));
        }

        [Fact]
        public void Valid_should_do_nothing_if_valid()
        {
            Guard.Valid(new MyValidatableValid(), "Parameter", () => "Message");
        }
    }
}