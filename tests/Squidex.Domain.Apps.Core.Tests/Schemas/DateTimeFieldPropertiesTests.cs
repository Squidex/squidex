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
using System.Reflection;
using FluentAssertions;
using NodaTime;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public class DateTimeFieldPropertiesTests
    {
        private readonly List<ValidationError> errors = new List<ValidationError>();

        [Fact]
        public void Should_not_add_error_if_sut_is_valid()
        {
            var sut = new DateTimeFieldProperties
            {
                MinValue = FutureDays(10),
                MaxValue = FutureDays(20),
                DefaultValue = FutureDays(15)
            };

            sut.Validate(errors);

            Assert.Empty(errors);
        }

        [Fact]
        public void Should_add_error_if_default_value_is_less_than_min()
        {
            var sut = new DateTimeFieldProperties { MinValue = FutureDays(10), DefaultValue = FutureDays(5) };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Default value must be greater than min value", "DefaultValue")
                });
        }

        [Fact]
        public void Should_add_error_if_default_value_is_greater_than_min()
        {
            var sut = new DateTimeFieldProperties { MaxValue = FutureDays(10), DefaultValue = FutureDays(15) };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Default value must be less than max value", "DefaultValue")
                });
        }

        [Fact]
        public void Should_add_error_if_min_greater_than_max()
        {
            var sut = new DateTimeFieldProperties { MinValue = FutureDays(10), MaxValue = FutureDays(5) };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Max value must be greater than min value", "MinValue", "MaxValue")
                });
        }

        [Fact]
        public void Should_add_error_if_editor_is_not_valid()
        {
            var sut = new DateTimeFieldProperties { Editor = (DateTimeFieldEditor)123 };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Editor is not a valid value", "Editor")
                });
        }

        [Fact]
        public void Should_add_error_if_calculated_default_value_is_not_valid()
        {
            var sut = new DateTimeFieldProperties { CalculatedDefaultValue = (DateTimeCalculatedDefaultValue)123 };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Calculated default value is not valid", "CalculatedDefaultValue")
                });
        }

        [Fact]
        public void Should_add_error_if_calculated_default_value_default_value_is_defined()
        {
            var sut = new DateTimeFieldProperties { CalculatedDefaultValue = DateTimeCalculatedDefaultValue.Now, DefaultValue = FutureDays(10) };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Calculated default value and default value cannot be used together", "CalculatedDefaultValue", "DefaultValue")
                });
        }

        [Fact]
        public void Should_provide_today_default_value()
        {
            var sut = new DateTimeFieldProperties { CalculatedDefaultValue = DateTimeCalculatedDefaultValue.Today };

            Assert.Equal(DateTime.UtcNow.Date.ToString("o"), sut.GetDefaultValue().ToString());
        }

        [Fact]
        public void Should_provide_now_default_value()
        {
            var sut = new DateTimeFieldProperties { CalculatedDefaultValue = DateTimeCalculatedDefaultValue.Now };

            Assert.Equal(DateTime.UtcNow.ToString("o").Substring(0, 16), sut.GetDefaultValue().ToString().Substring(0, 16));
        }

        [Fact]
        public void Should_provide_specific_default_value()
        {
            var sut = new DateTimeFieldProperties { DefaultValue = FutureDays(15) };

            Assert.Equal(FutureDays(15).ToString(), sut.GetDefaultValue());
        }

        [Fact]
        public void Should_set_or_freeze_sut()
        {
            var sut = new DateTimeFieldProperties();

            foreach (var property in sut.GetType().GetRuntimeProperties().Where(x => x.Name != "IsFrozen"))
            {
                var value =
                    property.PropertyType.GetTypeInfo().IsValueType ?
                        Activator.CreateInstance(property.PropertyType) :
                        null;

                property.SetValue(sut, value);

                var result = property.GetValue(sut);

                Assert.Equal(value, result);
            }

            sut.Freeze();

            foreach (var property in sut.GetType().GetRuntimeProperties().Where(x => x.Name != "IsFrozen"))
            {
                var value =
                    property.PropertyType.GetTypeInfo().IsValueType ?
                        Activator.CreateInstance(property.PropertyType) :
                        null;

                Assert.Throws<InvalidOperationException>(() =>
                {
                    try
                    {
                        property.SetValue(sut, value);
                    }
                    catch (Exception ex)
                    {
                        throw ex.InnerException;
                    }
                });
            }
        }

        private static Instant FutureDays(int days)
        {
            return Instant.FromDateTimeUtc(DateTime.UtcNow.Date.AddDays(days));
        }
    }
}