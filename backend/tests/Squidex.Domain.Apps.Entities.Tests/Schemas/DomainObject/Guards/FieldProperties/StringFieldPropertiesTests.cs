// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject.Guards.FieldProperties;

public class StringFieldPropertiesTests : IClassFixture<TranslationsFixture>
{
    [Fact]
    public void Should_add_error_if_min_length_greater_than_max()
    {
        var sut = new StringFieldProperties { MinLength = 10, MaxLength = 5 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Max length must be greater or equal to min length.", "MinLength", "MaxLength")
            });
    }

    [Fact]
    public void Should_not_add_error_if_min_length_equal_to_max_length()
    {
        var sut = new StringFieldProperties { MinLength = 2, MaxLength = 2 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        Assert.Empty(errors);
    }

    [Fact]
    public void Should_add_error_if_min_characters_greater_than_max()
    {
        var sut = new StringFieldProperties { MinCharacters = 10, MaxCharacters = 5 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Max characters must be greater or equal to min characters.", "MinCharacters", "MaxCharacters")
            });
    }

    [Fact]
    public void Should_not_add_error_if_min_characters_equal_to_max_characters()
    {
        var sut = new StringFieldProperties { MinCharacters = 2, MaxCharacters = 2 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        Assert.Empty(errors);
    }

    [Fact]
    public void Should_add_error_if_min_words_greater_than_max()
    {
        var sut = new StringFieldProperties { MinWords = 10, MaxWords = 5 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Max words must be greater or equal to min words.", "MinWords", "MaxWords")
            });
    }

    [Fact]
    public void Should_not_add_error_if_min_words_equal_to_max_words()
    {
        var sut = new StringFieldProperties { MinWords = 2, MaxWords = 2 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        Assert.Empty(errors);
    }

    [Fact]
    public void Should_add_error_if_radio_button_has_no_allowed_values()
    {
        var sut = new StringFieldProperties { Editor = StringFieldEditor.Radio };

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
        var sut = new StringFieldProperties { Editor = (StringFieldEditor)123 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Editor is not a valid value.", "Editor")
            });
    }

    [Fact]
    public void Should_add_error_if_content_type_is_not_valid()
    {
        var sut = new StringFieldProperties { ContentType = (StringContentType)123 };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Content type is not a valid value.", "ContentType")
            });
    }

    [Fact]
    public void Should_add_error_if_pattern_is_not_valid_regex()
    {
        var sut = new StringFieldProperties { Pattern = "[0-9{1}" };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Pattern is not a valid value.", "Pattern")
            });
    }

    [Theory]
    [InlineData(StringFieldEditor.Markdown)]
    [InlineData(StringFieldEditor.Radio)]
    [InlineData(StringFieldEditor.RichText)]
    [InlineData(StringFieldEditor.TextArea)]
    public void Should_add_error_if_inline_editing_is_not_allowed_for_editor(StringFieldEditor editor)
    {
        var sut = new StringFieldProperties { InlineEditable = true, Editor = editor, AllowedValues = ReadonlyList.Create("Value") };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
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
        var sut = new StringFieldProperties { InlineEditable = true, Editor = editor, AllowedValues = ReadonlyList.Create("Value") };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        Assert.Empty(errors);
    }
}
