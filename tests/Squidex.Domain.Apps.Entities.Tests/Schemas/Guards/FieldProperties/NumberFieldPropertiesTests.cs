﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas.Guards.FieldProperties
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

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Default value must be greater or equal to min value.", "DefaultValue")
                });
        }

        [Fact]
        public void Should_add_error_if_default_value_is_greater_than_min()
        {
            var sut = new NumberFieldProperties { MaxValue = 0, DefaultValue = 5 };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Default value must be less or equal to max value.", "DefaultValue")
                });
        }

        [Fact]
        public void Should_add_error_if_min_greater_than_max()
        {
            var sut = new NumberFieldProperties { MinValue = 10, MaxValue = 5 };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Max value must be greater than min value.", "MinValue", "MaxValue")
                });
        }

        [Fact]
        public void Should_add_error_if_allowed_values_and_max_value_is_specified()
        {
            var sut = new NumberFieldProperties { MaxValue = 10, AllowedValues = ReadOnlyCollection.Create(4d) };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Either allowed values or min and max value can be defined.", "AllowedValues", "MinValue", "MaxValue")
                });
        }

        [Fact]
        public void Should_add_error_if_allowed_values_and_min_value_is_specified()
        {
            var sut = new NumberFieldProperties { MinValue = 10, AllowedValues = ReadOnlyCollection.Create(4d) };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.Should().BeEquivalentTo(
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

            errors.Should().BeEquivalentTo(
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

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Editor is not a valid value.", "Editor")
                });
        }

        [Theory]
        [InlineData(NumberFieldEditor.Radio)]
        [InlineData(NumberFieldEditor.Stars)]
        public void Should_add_error_if_inline_editing_is_not_allowed_for_editor(NumberFieldEditor editor)
        {
            var sut = new NumberFieldProperties { InlineEditable = true, Editor = editor, AllowedValues = ReadOnlyCollection.Create(1.0) };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Inline editing is only allowed for dropdowns and input fields.", "InlineEditable", "Editor")
                });
        }

        [Theory]
        [InlineData(NumberFieldEditor.Input)]
        [InlineData(NumberFieldEditor.Dropdown)]
        public void Should_not_add_error_if_inline_editing_is_allowed_for_editor(NumberFieldEditor editor)
        {
            var sut = new NumberFieldProperties { InlineEditable = true, Editor = editor, AllowedValues = ReadOnlyCollection.Create(1.0) };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            Assert.Empty(errors);
        }
    }
}
