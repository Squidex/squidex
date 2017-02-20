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
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Core.Schemas
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

            Assert.Equal(0, errors.Count);
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
                    new ValidationError("Editor ist not a valid value", "Editor")
                });
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

        private static DateTimeOffset FutureDays(int days)
        {
            return DateTimeOffset.UtcNow.AddDays(days);
        }
    }
}