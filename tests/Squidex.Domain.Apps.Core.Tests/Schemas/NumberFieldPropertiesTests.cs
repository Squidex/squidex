// ==========================================================================
//  NumberFieldPropertiesTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public class NumberFieldPropertiesTests
    {
        private readonly List<ValidationError> errors = new List<ValidationError>();

        [Fact]
        public void Should_not_add_error_if_sut_is_valid()
        {
            var sut = new NumberFieldProperties
            {
                MinValue = 0,
                MaxValue = 100,
                DefaultValue = 5
            };

            sut.Validate(errors);

            Assert.Empty(errors);
        }

        [Fact]
        public void Should_add_error_if_default_value_is_less_than_min()
        {
            var sut = new NumberFieldProperties { MinValue = 10, DefaultValue = 5 };

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
            var sut = new NumberFieldProperties { MaxValue = 0, DefaultValue = 5 };

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
            var sut = new NumberFieldProperties { MinValue = 10, MaxValue = 5 };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Max value must be greater than min value", "MinValue", "MaxValue")
                });
        }

        [Fact]
        public void Should_add_error_if_allowed_values_and_max_value_is_specified()
        {
            var sut = new NumberFieldProperties { MaxValue = 10, AllowedValues = ImmutableList.Create<double>(4) };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Either allowed values or min and max value can be defined", "AllowedValues", "MinValue", "MaxValue")
                });
        }

        [Fact]
        public void Should_add_error_if_allowed_values_and_min_value_is_specified()
        {
            var sut = new NumberFieldProperties { MinValue = 10, AllowedValues = ImmutableList.Create<double>(4) };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Either allowed values or min and max value can be defined", "AllowedValues", "MinValue", "MaxValue")
                });
        }

        [Fact]
        public void Should_add_error_if_radio_button_has_no_allowed_values()
        {
            var sut = new NumberFieldProperties { Editor = NumberFieldEditor.Radio };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Radio buttons or dropdown list need allowed values", "AllowedValues")
                });
        }

        [Fact]
        public void Should_add_error_if_editor_is_not_valid()
        {
            var sut = new NumberFieldProperties { Editor = (NumberFieldEditor)123 };

            sut.Validate(errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Editor is not a valid value", "Editor")
                });
        }

        [Fact]
        public void Should_set_or_freeze_sut()
        {
            var sut = new NumberFieldProperties();

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
    }
}
