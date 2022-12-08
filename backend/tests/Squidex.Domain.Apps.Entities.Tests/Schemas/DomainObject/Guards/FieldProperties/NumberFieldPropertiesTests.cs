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

public class NumberFieldPropertiesTests : IClassFixture<TranslationsFixture>
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
    public void Should_add_error_if_min_value_greater_than_max_value()
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
    public void Should_add_error_if_inline_editing_is_not_allowed_for_editor(NumberFieldEditor editor)
    {
        var sut = new NumberFieldProperties { InlineEditable = true, Editor = editor, AllowedValues = ReadonlyList.Create(1.0) };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Inline editing is not allowed for Radio editor.", "InlineEditable", "Editor")
            });
    }

    [Theory]
    [InlineData(NumberFieldEditor.Input)]
    [InlineData(NumberFieldEditor.Dropdown)]
    [InlineData(NumberFieldEditor.Stars)]
    public void Should_not_add_error_if_inline_editing_is_allowed_for_editor(NumberFieldEditor editor)
    {
        var sut = new NumberFieldProperties { InlineEditable = true, Editor = editor, AllowedValues = ReadonlyList.Create(1.0) };

        var errors = FieldPropertiesValidator.Validate(sut).ToList();

        Assert.Empty(errors);
    }
}
