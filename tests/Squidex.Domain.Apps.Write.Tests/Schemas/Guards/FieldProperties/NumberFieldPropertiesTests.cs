// ==========================================================================
//  NumberFieldPropertiesTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Write.Schemas.Guards.FieldProperties
{
    public class NumberFieldPropertiesTests
    {
        [Fact]
        public void Should_not_add_error_if_sut_is_valid()
        {
            var sut = new NumberFieldProperties
            {
                MinValue = 0,
                MaxValue = 100,
                DefaultValue = 5
            };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            Assert.Empty(errors);
        }

        [Fact]
        public void Should_add_error_if_default_value_is_less_than_min()
        {
            var sut = new NumberFieldProperties { MinValue = 10, DefaultValue = 5 };

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
            var sut = new NumberFieldProperties { MaxValue = 0, DefaultValue = 5 };

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
            var sut = new NumberFieldProperties { MinValue = 10, MaxValue = 5 };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Max value must be greater than min value.", "MinValue", "MaxValue")
                });
        }

        [Fact]
        public void Should_add_error_if_allowed_values_and_max_value_is_specified()
        {
            var sut = new NumberFieldProperties { MaxValue = 10, AllowedValues = ImmutableList.Create<double>(4) };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Either allowed values or min and max value can be defined.", "AllowedValues", "MinValue", "MaxValue")
                });
        }

        [Fact]
        public void Should_add_error_if_allowed_values_and_min_value_is_specified()
        {
            var sut = new NumberFieldProperties { MinValue = 10, AllowedValues = ImmutableList.Create<double>(4) };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Either allowed values or min and max value can be defined.", "AllowedValues", "MinValue", "MaxValue")
                });
        }

        [Fact]
        public void Should_add_error_if_radio_button_has_no_allowed_values()
        {
            var sut = new NumberFieldProperties { Editor = NumberFieldEditor.Radio };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Radio buttons or dropdown list need allowed values.", "AllowedValues")
                });
        }

        [Fact]
        public void Should_add_error_if_editor_is_not_valid()
        {
            var sut = new NumberFieldProperties { Editor = (NumberFieldEditor)123 };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Editor is not a valid value.", "Editor")
                });
        }
    }
}
