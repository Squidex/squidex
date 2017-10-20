// ==========================================================================
//  DateTimeFieldPropertiesTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NodaTime;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Write.Schemas.Guards.FieldProperties
{
    public class DateTimeFieldPropertiesTests
    {
        [Fact]
        public void Should_not_add_error_if_sut_is_valid()
        {
            var sut = new DateTimeFieldProperties
            {
                MinValue = FutureDays(10),
                MaxValue = FutureDays(20),
                DefaultValue = FutureDays(15)
            };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            Assert.Empty(errors);
        }

        [Fact]
        public void Should_add_error_if_default_value_is_less_than_min()
        {
            var sut = new DateTimeFieldProperties { MinValue = FutureDays(10), DefaultValue = FutureDays(5) };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Default value must be greater than min value.", "DefaultValue")
                });
        }

        [Fact]
        public void Should_add_error_if_default_value_is_greater_than_min()
        {
            var sut = new DateTimeFieldProperties { MaxValue = FutureDays(10), DefaultValue = FutureDays(15) };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Default value must be less than max value.", "DefaultValue")
                });
        }

        [Fact]
        public void Should_add_error_if_min_greater_than_max()
        {
            var sut = new DateTimeFieldProperties { MinValue = FutureDays(10), MaxValue = FutureDays(5) };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Max value must be greater than min value.", "MinValue", "MaxValue")
                });
        }

        [Fact]
        public void Should_add_error_if_editor_is_not_valid()
        {
            var sut = new DateTimeFieldProperties { Editor = (DateTimeFieldEditor)123 };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Editor is not a valid value.", "Editor")
                });
        }

        [Fact]
        public void Should_add_error_if_calculated_default_value_is_not_valid()
        {
            var sut = new DateTimeFieldProperties { CalculatedDefaultValue = (DateTimeCalculatedDefaultValue)123 };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Calculated default value is not valid.", "CalculatedDefaultValue")
                });
        }

        [Fact]
        public void Should_add_error_if_calculated_default_value_default_value_is_defined()
        {
            var sut = new DateTimeFieldProperties { CalculatedDefaultValue = DateTimeCalculatedDefaultValue.Now, DefaultValue = FutureDays(10) };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Calculated default value and default value cannot be used together.", "CalculatedDefaultValue", "DefaultValue")
                });
        }

        private static Instant FutureDays(int days)
        {
            return Instant.FromDateTimeUtc(DateTime.UtcNow.Date.AddDays(days));
        }
    }
}