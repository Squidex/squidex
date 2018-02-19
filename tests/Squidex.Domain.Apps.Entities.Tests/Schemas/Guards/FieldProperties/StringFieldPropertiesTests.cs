// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas.Guards.FieldProperties
{
    public class StringFieldPropertiesTests
    {
        [Fact]
        public void Should_add_error_if_min_greater_than_max()
        {
            var sut = new StringFieldProperties { MinLength = 10, MaxLength = 5 };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Max length must be greater than min length.", "MinLength", "MaxLength")
                });
        }

        [Fact]
        public void Should_add_error_if_allowed_values_and_max_value_is_specified()
        {
            var sut = new StringFieldProperties { MinLength = 10, AllowedValues = ImmutableList.Create("4") };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Either allowed values or min and max length can be defined.", "AllowedValues", "MinLength", "MaxLength")
                });
        }

        [Fact]
        public void Should_add_error_if_allowed_values_and_min_value_is_specified()
        {
            var sut = new StringFieldProperties { MaxLength = 10, AllowedValues = ImmutableList.Create("4") };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Either allowed values or min and max length can be defined.", "AllowedValues", "MinLength", "MaxLength")
                });
        }

        [Fact]
        public void Should_add_error_if_radio_button_has_no_allowed_values()
        {
            var sut = new StringFieldProperties { Editor = StringFieldEditor.Radio };

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
            var sut = new StringFieldProperties { Editor = (StringFieldEditor)123 };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Editor is not a valid value.", "Editor")
                });
        }

        [Fact]
        public void Should_add_error_if_pattern_is_not_valid_regex()
        {
            var sut = new StringFieldProperties { Pattern = "[0-9{1}" };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Pattern is not a valid expression.", "Pattern")
                });
        }

        [Theory]
        [InlineData(StringFieldEditor.Markdown)]
        [InlineData(StringFieldEditor.Radio)]
        [InlineData(StringFieldEditor.RichText)]
        [InlineData(StringFieldEditor.TextArea)]
        public void Should_add_error_if_inline_editing_is_not_allowed_for_editor(StringFieldEditor editor)
        {
            var sut = new StringFieldProperties { InlineEditable = true, Editor = editor, AllowedValues = ImmutableList.Create("Value") };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Inline editing is only allowed for dropdowns, slugs and input fields.", "InlineEditable", "Editor")
                });
        }

        [Theory]
        [InlineData(StringFieldEditor.Dropdown)]
        [InlineData(StringFieldEditor.Input)]
        [InlineData(StringFieldEditor.Slug)]
        public void Should_not_add_error_if_inline_editing_is_allowed_for_editor(StringFieldEditor editor)
        {
            var sut = new StringFieldProperties { InlineEditable = true, Editor = editor, AllowedValues = ImmutableList.Create("Value") };

            var errors = FieldPropertiesValidator.Validate(sut).ToList();

            Assert.Empty(errors);
        }
    }
}
